#!/usr/bin/env bash
set -euo pipefail

API_URL="${API_URL:-http://127.0.0.1:5110}"
DEPOSIT_AMOUNT="${DEPOSIT_AMOUNT:-100}"
DOTNET_CMD="${DOTNET_CMD:-dotnet}"
DOTNET_EF_VERSION="${DOTNET_EF_VERSION:-10.0.7}"
DOWN_DEPS=false
RESET_DATA=false

usage() {
  cat <<USAGE
Usage: $0 [--down-deps] [--reset-data]

Runs the local account smoke flow:
  - starts docker compose dependencies when they are not running
  - applies EF Core migrations
  - starts the API on ${API_URL}
  - creates an account
  - deposits ${DEPOSIT_AMOUNT}
  - verifies the account balance
  - verifies RabbitMQ queue messages

Options:
  --down-deps   Stop docker compose dependencies after the smoke test.
  --reset-data  Remove docker compose volumes before starting dependencies.
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --down-deps)
      DOWN_DEPS=true
      shift
      ;;
    --reset-data)
      RESET_DATA=true
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
API_LOG="$(mktemp "${TMPDIR:-/tmp}/ejemplo-event-sourcing-api.XXXXXX")"
API_PID=""

if [[ "${DOTNET_CMD}" == */* ]]; then
  DOTNET_CMD_DIR="$(cd "$(dirname "${DOTNET_CMD}")" && pwd)"
  export DOTNET_ROOT="${DOTNET_ROOT:-${DOTNET_CMD_DIR}}"
  export PATH="${DOTNET_CMD_DIR}:${PATH}"
fi

log() {
  printf '[smoke] %s\n' "$*"
}

fail() {
  printf '[smoke] ERROR: %s\n' "$*" >&2
  if [[ -f "${API_LOG}" ]]; then
    printf '[smoke] API log: %s\n' "${API_LOG}" >&2
    tail -n 80 "${API_LOG}" >&2 || true
  fi
  exit 1
}

cleanup() {
  if [[ -n "${API_PID}" ]] && kill -0 "${API_PID}" 2>/dev/null; then
    log "Stopping API process ${API_PID}"
    kill "${API_PID}" 2>/dev/null || true
    wait "${API_PID}" 2>/dev/null || true
  fi

  if [[ "${DOWN_DEPS}" == "true" ]]; then
    log "Stopping docker compose dependencies"
    if [[ "${RESET_DATA}" == "true" ]]; then
      docker compose -f "${REPO_ROOT}/docker-compose.yml" down -v
    else
      docker compose -f "${REPO_ROOT}/docker-compose.yml" down
    fi
  fi
}
trap cleanup EXIT

require_command() {
  local command_name="$1"
  if ! command -v "${command_name}" >/dev/null 2>&1; then
    fail "Required command not found: ${command_name}"
  fi
}

service_is_running() {
  local service="$1"
  local container_id
  container_id="$(docker compose -f "${REPO_ROOT}/docker-compose.yml" ps -q "${service}")"
  [[ -n "${container_id}" ]] && [[ "$(docker inspect -f '{{.State.Running}}' "${container_id}" 2>/dev/null)" == "true" ]]
}

wait_for_service_health() {
  local service="$1"
  local container_id
  local status

  container_id="$(docker compose -f "${REPO_ROOT}/docker-compose.yml" ps -q "${service}")"
  [[ -n "${container_id}" ]] || fail "No container found for service ${service}"

  for _ in {1..60}; do
    status="$(docker inspect -f '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "${container_id}")"
    if [[ "${status}" == "healthy" ]] || [[ "${status}" == "running" ]]; then
      log "${service} is ${status}"
      return 0
    fi
    sleep 1
  done

  fail "Timed out waiting for ${service} health"
}

resolve_dotnet_ef() {
  local tool_path="/tmp/dotnet-tools-${DOTNET_EF_VERSION}"

  if [[ "$("${DOTNET_CMD}" ef --version 2>/dev/null || true)" == "${DOTNET_EF_VERSION}"* ]]; then
    DOTNET_EF=("${DOTNET_CMD}" ef)
    return 0
  fi

  if [[ -x "${tool_path}/dotnet-ef" ]]; then
    DOTNET_EF=("${tool_path}/dotnet-ef")
    return 0
  fi

  log "Installing dotnet-ef ${DOTNET_EF_VERSION} into ${tool_path}"
  "${DOTNET_CMD}" tool install dotnet-ef \
    --version "${DOTNET_EF_VERSION}" \
    --tool-path "${tool_path}" >/dev/null
  DOTNET_EF=("${tool_path}/dotnet-ef")
}

wait_for_api() {
  for _ in {1..60}; do
    if curl -fsS "${API_URL}/HealthCheck" >/dev/null 2>&1; then
      log "API is listening on ${API_URL}"
      return 0
    fi

    if [[ -n "${API_PID}" ]] && ! kill -0 "${API_PID}" 2>/dev/null; then
      fail "API process exited before becoming ready"
    fi

    sleep 1
  done

  fail "Timed out waiting for API at ${API_URL}"
}

queue_ready_messages() {
  local queue_name="$1"
  docker compose -f "${REPO_ROOT}/docker-compose.yml" exec -T rabbitmq \
    rabbitmqctl list_queues -p example-vhost name messages_ready 2>/dev/null \
    | awk -v queue="${queue_name}" '$1 == queue { print $2 }'
}

assert_queue_messages_at_least() {
  local queue_name="$1"
  local expected="$2"
  local actual

  actual="$(queue_ready_messages "${queue_name}")"
  [[ -n "${actual}" ]] || fail "Queue ${queue_name} was not found"

  if (( actual < expected )); then
    fail "Queue ${queue_name} has ${actual} ready messages, expected at least ${expected}"
  fi

  log "Queue ${queue_name} has ${actual} ready messages"
}

require_command docker
require_command "${DOTNET_CMD}"
require_command curl

cd "${REPO_ROOT}"

if [[ "${RESET_DATA}" == "true" ]]; then
  log "Resetting docker compose data"
  docker compose down -v
fi

if service_is_running postgres && service_is_running rabbitmq; then
  log "Docker compose dependencies are already running"
else
  log "Starting docker compose dependencies"
  docker compose up -d --build
fi

wait_for_service_health postgres
wait_for_service_health rabbitmq

resolve_dotnet_ef

log "Applying EF Core migrations"
"${DOTNET_EF[@]}" database update \
  --project EjemploEventSourcing.Infrastructure \
  --startup-project EjemploEventSourcing.API

log "Starting API on ${API_URL}"
"${DOTNET_CMD}" run --no-launch-profile \
  --project EjemploEventSourcing.API \
  --urls "${API_URL}" >"${API_LOG}" 2>&1 &
API_PID="$!"

wait_for_api

log "Creating account"
if ! account_response="$(curl -fsS -X POST "${API_URL}/CreateAccount")"; then
  fail "CreateAccount request failed"
fi
account_id="${account_response%\"}"
account_id="${account_id#\"}"
[[ -n "${account_id}" ]] || fail "CreateAccount did not return an account id"
log "Created account ${account_id}"

log "Depositing ${DEPOSIT_AMOUNT}"
if ! curl -fsS -X POST "${API_URL}/DepositAmount" \
  -H "Content-Type: application/json" \
  -d "{\"accountId\":\"${account_id}\",\"depositAmount\":${DEPOSIT_AMOUNT}}" >/dev/null; then
  fail "DepositAmount request failed"
fi

log "Reading account ${account_id}"
if ! account_json="$(curl -fsS "${API_URL}/GetAccountById/${account_id}")"; then
  fail "GetAccountById request failed"
fi
case "${account_json}" in
  *"\"id\":\"${account_id}\""*"\"balance\":${DEPOSIT_AMOUNT}"*)
    log "Balance verified: ${DEPOSIT_AMOUNT}"
    ;;
  *)
    fail "Unexpected account response: ${account_json}"
    ;;
esac

assert_queue_messages_at_least AccountCreated 1
assert_queue_messages_at_least AmountDeposited 1
assert_queue_messages_at_least Events 2

log "Smoke test passed"
