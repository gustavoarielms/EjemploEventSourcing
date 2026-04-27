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

## Ejecutar

```bash
dotnet run --project EjemploEventSourcing.API
```

Swagger queda disponible en la URL configurada por ASP.NET Core para el proyecto.

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
