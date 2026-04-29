# Ejemplo Event Sourcing

Ejemplo de Event Sourcing con ASP.NET Core, PostgreSQL como event store y RabbitMQ para publicar eventos.

## Requisitos

- .NET SDK 8
- Docker

## Dependencias locales

Levantar PostgreSQL y RabbitMQ:

```bash
docker compose up -d
```

Servicios expuestos:

- PostgreSQL: `localhost:5432`
- RabbitMQ: `localhost:5672`
- RabbitMQ Management: <http://localhost:15672>

Credenciales RabbitMQ:

- usuario: `guest`
- password: `nimda`
- vhost: `example-vhost`

La configuración coincide con `EjemploEventSourcing.API/appsettings.json`.

## Base de datos

Aplicar migraciones:

```bash
dotnet ef database update \
  --project EjemploEventSourcing.Infrastructure \
  --startup-project EjemploEventSourcing.API
```

Si `dotnet ef` no está instalado:

```bash
dotnet tool install --global dotnet-ef --version 8.0.8
```

## Ejecutar

```bash
dotnet run --no-launch-profile \
  --project EjemploEventSourcing.API \
  --urls http://127.0.0.1:5110
```

Swagger queda disponible en <http://127.0.0.1:5110>.

## Smoke test

El flujo local completo se puede ejecutar con:

```bash
script/smoke/local-account-flow.sh
```

Para ejecutar desde una base limpia y apagar las dependencias al finalizar:

```bash
script/smoke/local-account-flow.sh --reset-data --down-deps
```

El script levanta dependencias si no estan corriendo, aplica migraciones, inicia la API en `http://127.0.0.1:5110`, crea una cuenta, deposita `100`, valida el balance y verifica mensajes en RabbitMQ.

### Smoke test manual

Crear cuenta:

```bash
curl -X POST http://127.0.0.1:5110/CreateAccount
```

Depositar en la cuenta creada:

```bash
curl -X POST http://127.0.0.1:5110/DepositAmount \
  -H "Content-Type: application/json" \
  -d '{"accountId":"ACCOUNT_ID","depositAmount":100}'
```

Consultar la cuenta:

```bash
curl http://127.0.0.1:5110/GetAccountById/ACCOUNT_ID
```

## Validar

```bash
dotnet test EjemploEventSourcing.sln
dotnet build EjemploEventSourcing.sln
```

## Apagar dependencias

```bash
docker compose down
```

Para borrar también los datos locales de PostgreSQL:

```bash
docker compose down -v
```
