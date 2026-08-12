# AGENTS.md

Guidelines for coding agents working in this repository.

## Mission

Build and maintain a focused MCP server for online grocery workflows.

Prioritize:
1. Correct MCP contracts
2. Simple provider boundaries
3. Deterministic validation
4. Honest grocery/cart limitations
5. Small issue-driven delivery

The current product direction is an online grocery assistant with a VTEX-backed provider, fixture-backed CI, and WhatsApp as a thin conversational surface over MCP capabilities.

## Mandatory Constraints

- This repo is MCP-first. Keep WhatsApp code as an integration layer unless a real runtime, deploy, secrets, or release boundary justifies a split.
- Do not implement login, checkout completion, payment, or automatic purchase.
- Do not pass cookies, tokens, email addresses, checkout credentials, or payment data through prompts, logs, MCP payloads, or WhatsApp flows.
- `build_basket` is read-only. Real cart mutation belongs in `add_basket_to_cart` and must require a configured cart identity.
- The only current cart identity strategy is `guest-order-form` with `GROCERY_CART_ORDER_FORM_ID`.
- If cart identity is missing or invalid, return controlled warnings and avoid calling the provider mutation path.
- Do not claim that a browser cart URL is a reliable handoff for an API-created VTEX `orderFormId`.
- Do not invent geolocation. `list_pickup_points` requires trusted latitude and longitude from the client, GPS, browser, mobile app, geocoder, or another explicit source.
- CI and smoke tests must use the fixture provider, not live VTEX traffic.
- Live VTEX checks are manual diagnostics, not required for deterministic validation.

## MVP Boundaries

Allowed:
- Product search and detail lookup
- Location normalization
- Delivery estimation
- Pickup-point lookup from trusted coordinates
- Basket preview and matching
- Explicit basket-to-cart mutation
- WhatsApp command interpretation over approved MCP/backend operations
- Fixture provider coverage for tests and smoke
- Focused VTEX provider improvements
- Repo-local docs for changed behavior

Do not add:
- Checkout or payment execution
- User credential/session capture
- Provider-generic abstractions without a second real provider need
- Kubernetes, Kafka, CQRS, event sourcing, or distributed orchestration
- Generic repository patterns or broad DDD layers
- Multi-tenant architecture
- Plugin systems
- Repo splitting for WhatsApp before a real operational boundary exists

## Project Workflow

The GitHub Project at `https://github.com/orgs/patxa/projects/1/views/1` is the planning source of truth. Before issue or card work:

1. Fetch and inspect current repo state.
2. Read the live issue and its subissues.
3. Read the Project V2 item, fields, and status if your token has `read:project`.
4. If Project access is missing, say that clearly and do not guess column state.

When the user points to an issue with phrases like `Toma este issue`, `Refina esta tarea`, or `avanza con esta tarea`, work the real GitHub issue instead of producing a chat-only plan.

For issue refinement:
- Keep Spanish issue/PR content in Spanish when the issue is already Spanish.
- Put important scope limits directly in the issue body.
- Split large work into native GitHub subissues by concrete use case or adapter.
- Avoid mixing strategy, architecture decisions, and implementation in one task.
- Prefer one scoped PR per issue or refactor.
- Keep Project V2 status, assignee, parent issue, and child issue state aligned when the request includes board/workflow updates.

## Architecture Guidelines

### Core

`src/core` owns domain types, matching rules, and unit helpers.

Rules:
- Keep it pure.
- Do not import NestJS, MCP, providers, or WhatsApp code.
- Put reusable grocery behavior here only when it is provider-independent.

### Application

`src/application` owns use cases and application-facing ports.

Rules:
- Keep one clear use case per behavior.
- Use capability ports that match actual use-case needs.
- Keep `GroceryUseCases` as the MCP-facing facade.
- Do not add interfaces unless they protect a real boundary already present in the code.

### Providers

`src/providers` owns infrastructure adapters.

Rules:
- VTEX-specific HTTP, endpoint construction, normalization, simulation, cart mutation, and `unitMultiplier` handling belong in VTEX adapters.
- Fixture behavior must stay deterministic and safe for CI.
- Do not let MCP schemas or WhatsApp command formats leak into provider code.

### MCP

`src/mcp` owns schemas, tool handlers, and server wiring.

Rules:
- Preserve existing tool names and response contracts unless the task explicitly changes public API.
- Validate inputs with Zod schemas.
- Delegate behavior to `GroceryUseCases`, not directly to provider adapters.
- Keep stdio runtime behavior stable.

### WhatsApp

`src/whatsapp` owns conversational integration.

Rules:
- Keep WhatsApp as a thin layer over the MCP/backend command contract.
- Use the strict command schema for LLM output.
- `buscar` maps to `search_products`; `agregar` maps to `add_basket_to_cart`.
- Cart wrappers such as view, update quantity, and remove are backend operations, not prompt-side provider calls.
- `unknown` and `ambiguous` commands must not mutate cart state.
- Preserve the explicit boundary: the WhatsApp MVP does not execute payment or checkout.

## Coding Guidelines

- Prefer explicit code over abstractions.
- Keep changes surgical and tied to the requested issue.
- Match existing ESM TypeScript and NestJS patterns.
- Use Node 20-compatible APIs.
- Do not refactor adjacent code unless the request requires it.
- Do not change public MCP contracts accidentally.
- Keep product matching conservative: exact/category-aware checks are safer than broad query guesses.
- Treat fresh goods quantity conversion carefully; VTEX item counts may depend on `unitMultiplier`.
- Do not log secrets, credentials, cookies, emails, payment data, or raw invalid cart identifiers.
- Remove unused imports or code introduced by your own change.

## Validation

Default local validation:

```bash
npm run check
npm run build
npm test
npm run smoke:mcp
git diff --check
```

Notes:
- `npm run smoke:mcp` expects `dist/index.js`, so run `npm run build` first.
- The smoke uses `GROCERY_PROVIDER=fixture` and must not call VTEX or mutate real carts.
- Use `npm run probe -- "<query>" <postalCode>` only for manual live-provider diagnostics.
- If a change touches only docs, say why code validation was not run.
- If a change touches WhatsApp command behavior, add or update WhatsApp tests.
- If a change touches MCP schemas or tool handlers, add or update contract tests and smoke coverage when appropriate.

## Git Workflow

Branches:
- `feat/<issue-or-name>`
- `fix/<issue-or-name>`
- `docs/<issue-or-name>`
- `refactor/<issue-or-name>`
- `test/<issue-or-name>`
- `chore/<issue-or-name>`

Use lowercase kebab-case.

Commits and PR titles:
- Use Conventional Commits.
- The PR title subject must start with lowercase.
- Do not use `[codex]` prefixes.
- If behavior, API, exports, or config changes, do not label it as `refactor:`.

PRs:
- Keep PRs focused.
- Do not mix refactors with feature delivery.
- Use the repository PR template.
- Include verification steps.
- Update docs when observable behavior changes.
- Prefer squash merge so release tooling reads the PR title.

## Clarification Rules

Before coding, state assumptions when they affect:
- Architecture
- Persistence
- Public MCP APIs
- GitHub issue/card workflow
- Business logic
- UX or WhatsApp behavior

Ask before proceeding when multiple valid implementations would produce materially different outcomes.

Do not ask when:
- Existing repo patterns clearly define the solution.
- The missing detail is cosmetic.
- A safe default is obvious from the issue, README, tests, or existing code.

## Success Criteria

For every non-trivial task:

1. Define the intended behavior.
2. Make the smallest code/doc change that satisfies it.
3. Add or update focused tests when behavior changes.
4. Run the relevant validation commands.
5. Report exactly what changed, what was verified, and any remaining limitation.

Every changed line should trace back to the requested issue, project card, bug, or documented repo boundary.

<!-- multi-sdd-team: begin -->
# Global Codex Orchestrator Policy

The main Codex session is the SDD orchestrator by default.

When the user asks for a feature, fix, refactor, migration, audit, or implementation task, classify the work before executing. Do not spawn an `orchestrator` subagent for classification unless the user explicitly asks for that. The active session owns routing, sequencing, delegation, and final integration.

Use the pipeline contract at `~/.codex/pipeline.json` for delegation order. If the project has `pipeline.json`, prefer the project copy. The pipeline is mandatory for multi-agent work. Current mode is `demo_fast`.

## Global Demo Fast Mode

For demo-speed multi-agent work, optimize for quick iteration:

- Spawn specialist agents with Codex `fast` enabled by default.
- Agent TOML files use `service_tier = "fast"` and `[features].fast_mode = true`; reasoning effort stays medium/high depending on the role.
- Do not call the `hacker` agent during demo work unless the user explicitly asks for a security audit.
- Use `tester_reviewer` as the review/validation step when needed. If the reviewer reports required changes, the main orchestrator applies those changes directly instead of calling the builder/implementer again.
- Prefer this fast demo chain: `planner` only if the implementation shape is unclear, then `implementer`, then `tester_reviewer`, then main-session fixes.

If the chosen strategy uses subagents, the active session coordinates, waits, reviews, and integrates. It must not implement, inspect, scaffold, prepare, verify, or otherwise advance the delegated task locally while subagents are running.

After spawning any subagent, stop local work on the task and wait for the relevant handoff before taking the next implementation, inspection, or validation step. During that waiting period, the active session may only:

- tell the user which agents were spawned and why
- wait for agent results
- answer a direct user status question

Do not run shell commands, read files, inspect inputs, search the repo, create files, edit files, start servers, or validate behavior while subagents are working unless the user explicitly authorizes parallel local work.

If the user says "sos el orquestador", "actua como orchestrator", or similar, treat that as orchestration-first mode: classify, delegate, supervise, and avoid building the feature locally while specialist agents are handling it.

## Available Specialist Agents

- `explorer`: read-only codebase reconnaissance.
- `planner`: sequenced implementation planning.
- `documentator`: functional and technical specs under `docs/`.
- `implementer`: focused TDD implementation.
- `tester_reviewer`: report-only static/E2E validation.
- `hacker`: passive security audit only when explicitly requested or when security review is required by the task.

## Strategy Options

- `INLINE`
- `SUBAGENT_SINGLE`
- `SUBAGENT_CHAIN`
- `SDD_INLINE`
- `SDD_SUBAGENTS`

## Routing Rules

- `R1 INLINE`: all are true: <=2 production files, <=30 LOC, no new API/schema/dependency, no user-visible behavior change, no new tests, or the task is a question/exploration.
- `R2 SUBAGENT_SINGLE`: bounded reconnaissance, focused security audit, one-shot documentation, or review of one file.
- `R3 SUBAGENT_CHAIN`: multi-step work with a clear spec, mechanical refactor/migration, complex bug without design alternatives, or hotfix.
- `R4 SDD_INLINE`: SDD checklist fires and scope is <=5 files, one module, one session.
- `R5 SDD_SUBAGENTS`: SDD checklist fires and any is true: >5 files, >1 module, new API/schema, security-sensitive, likely >2h or >150 LOC, user explicitly asks for spec/design, or there are multiple plausible designs.

## SDD Checklist

1. Is expected behavior ambiguous?
2. Are there at least two reasonable designs?
3. Does it change an observable contract such as API, schema, CLI, event, or UI flow?
4. Is risk above low, involving data, security, money, or irreversible state?
5. Does it need acceptance criteria verifiable by another agent or human?

If at least two checklist items are yes, SDD is justified. If zero or one are yes, skip SDD.

## Anti-SDD

- Bug fix with clear root cause and patch <30 LOC.
- Cosmetic, typo, copy, or formatting changes.
- Conceptual question or docs lookup.
- Mechanical refactor with no behavior change.
- Approved spec already exists: use `implementer` -> `tester_reviewer`.
- User says "sin spec", "rapido", "just do it", or "hotfix".

## Output

For substantial work, briefly state:

1. strategy
2. rule fired
3. rationale
4. handoff plan
5. expected validation

Then execute the chosen path. For small `INLINE` work, keep classification implicit unless it helps the user.

## Execution Discipline

- `INLINE`: the main session may implement directly.
- `SUBAGENT_SINGLE`: delegate the scoped task, then wait. Do not inspect, scaffold, implement, or validate locally until the agent returns.
- `SUBAGENT_CHAIN`: run the chain and wait for each needed handoff. Do not work ahead locally.
- `SDD_INLINE`: the main session may write the spec/plan and implement because scope is intentionally inline.
- `SDD_SUBAGENTS`: delegate spec/planning/implementation/review to specialists. The main session coordinates and integrates only after results return. Never build a "base", inspect data, or prepare files locally while those specialists run.

## Mandatory SDD_SUBAGENTS Order

1. `explorer`
2. `documentator`, only after `explorer` returns
3. `planner`, only after `documentator` returns
4. `hacker`, skipped by default in demo_fast mode; only when explicitly requested or required by security-sensitive work, and only after `planner` returns
5. `implementer`, only after `planner` and optional `hacker` return
6. `tester_reviewer`, only after `implementer` returns
7. `main_session` integrates and directly fixes reviewer findings, only after `tester_reviewer` returns
<!-- multi-sdd-team: end -->
