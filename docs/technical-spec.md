# Especificación técnica: biblioteca Event Sourcing Core v1

## Arquitectura propuesta

Se incorporará un único proyecto Core dirigido a .NET 10 y con referencias exclusivas a la BCL. El proyecto contiene el modelo de agregado, los contratos de store y el repositorio genérico. No referencia proyectos del ejemplo, EF Core, Npgsql, RabbitMQ ni bibliotecas de serialización.

En esta entrega el Core queda operativo de manera standalone. `Account` permanece en el proyecto de ejemplo sobre el agregado legacy; su migración a consumidor del Core se difiere porque requiere adaptar conjuntamente sus eventos, interactors, mappers y bordes de persistencia/publicación. La infraestructura actual podrá convertirse más adelante en un adaptador que implemente el puerto `IEventStore<string>`. La publicación RabbitMQ permanece en su borde actual y no es invocada por Core.

Flujo de dependencia:

```text
Agregado consumidor futuro ──> EventSourcing.Core <── adaptador de persistencia futuro
                                      ^
                                      └── EventSourcingRepository

Account legacy / RabbitMQ / JSON / EventsMapper: fuera de Core en esta entrega
```

## API pública propuesta

Los nombres de espacio y de proyecto definitivos deben seguir la convención elegida al crear el proyecto. Las siguientes firmas son el contrato v1; no se agregan campos de metadata ni serialización hasta que exista un caso de uso confirmado.

```csharp
public interface IDomainEvent
{
}

public sealed record EventEnvelope<TId>(
    TId AggregateId,
    long Version,
    IDomainEvent Event);

public interface IEventStore<TId>
{
    Task<IReadOnlyList<EventEnvelope<TId>>> ReadAsync(
        TId aggregateId,
        CancellationToken cancellationToken = default);

    Task AppendAsync(
        TId aggregateId,
        long expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default);
}

public abstract class AggregateRoot<TId>
{
    public TId Id { get; }
    public long Version { get; }
    public long PersistedVersion { get; }
    public IReadOnlyList<IDomainEvent> PendingEvents { get; }

    protected void Initialize(TId id);
    protected void Raise(IDomainEvent @event);
    protected abstract void Apply(IDomainEvent @event);

    // Invocados por el repositorio del Core, no por el consumidor.
    internal void Replay(IReadOnlyList<EventEnvelope<TId>> history);
    internal void AcceptChanges();
}

public sealed class EventSourcingRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
{
    public EventSourcingRepository(
        IEventStore<TId> eventStore,
        Func<TAggregate> aggregateFactory);

    public Task<TAggregate?> LoadAsync(
        TId aggregateId,
        CancellationToken cancellationToken = default);

    public Task SaveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);
}

public sealed class EventStoreConcurrencyException<TId> : Exception
{
    public TId AggregateId { get; }
    public long ExpectedVersion { get; }
    public long ActualVersion { get; }
}
```

`IDomainEvent` es intencionalmente un marcador tipado. La decisión evita que Core imponga serialización, formato de payload, nomenclatura de eventos o un registro de tipos. Cada adaptador de persistencia decide cómo almacenar y reconstruir el evento; antes de llamar a `Replay`, debe entregar el `IDomainEvent` concreto correcto.

`ReadAsync` retorna un historial vacío cuando el stream no existe. Por ello `LoadAsync` retorna `null` en ese caso; la creación de un agregado ocurre en el dominio mediante una fábrica o constructor propio, no en el repositorio. El `aggregateFactory` permite reconstruir agregados cuyo constructor no es público ni trivial sin imponer `new()`.

## Componentes y responsabilidades

| Componente | Responsabilidad | No responsabilidad |
| --- | --- | --- |
| `IDomainEvent` | Identificar un evento de dominio apto para el Core. | Datos de infraestructura o serialización. |
| `EventEnvelope<TId>` | Representar un evento ya almacenado con identificador y versión. | Metadata, nombre de tipo o bytes persistidos. |
| `AggregateRoot<TId>` | Aplicar eventos, mantener versiones y exponer pendientes. | Reglas de negocio concretas, I/O o publicación. |
| `IEventStore<TId>` | Leer historial y hacer append atómico con concurrencia optimista. | Construir agregados o convertir JSON a eventos. |
| `EventSourcingRepository` | Coordinar carga/replay y save/append/accept. | Transacciones multi-stream o reintentos. |
| Adaptador futuro | Implementar el contrato para EF/PostgreSQL y su mapeo. | Modificar las invariantes del Core. |

## Algoritmos requeridos

### `AggregateRoot<TId>.Raise`

1. Rechaza un evento nulo.
2. Ejecuta `Apply(event)` definido por el consumidor.
3. Agrega el evento a `PendingEvents`.
4. Incrementa `Version`.

Si `Apply` lanza, no agrega el evento ni incrementa la versión.

### `AggregateRoot<TId>.Replay`

1. Sólo se ejecuta sobre un agregado sin inicialización previa de historial ni pendientes.
2. Valida que cada envelope tenga el mismo `AggregateId`, que las versiones comiencen en 1 y sean consecutivas y ascendentes.
3. Ejecuta `Apply(envelope.Event)` por cada envelope, sin invocar `Raise`.
4. Establece `Id`, `Version` y `PersistedVersion` a la versión final, y deja `PendingEvents` vacío.

Un historial vacío no llama a `Apply`; el repositorio devuelve `null` y no crea una instancia observable como agregado existente.

### `EventSourcingRepository.SaveAsync`

1. Si no hay pendientes, termina sin llamar al store.
2. Invoca `AppendAsync(aggregate.Id, aggregate.PersistedVersion, aggregate.PendingEvents, cancellationToken)`.
3. Sólo cuando `AppendAsync` completa sin excepción, llama a `AcceptChanges` para igualar `PersistedVersion` a `Version` y vaciar pendientes.
4. Propaga una excepción del store sin mutar el estado de aceptación del agregado.

El store calcula las versiones asignadas a los nuevos eventos como `expectedVersion + 1 ... expectedVersion + N`; el Core no entrega versiones de escritura para evitar que el agregado y el proveedor las dupliquen.

## Contratos de adaptador y errores

Un adaptador de `IEventStore<TId>` debe:

- Validar identificadores y `expectedVersion` no negativo según el tipo de identificador acordado por el consumidor.
- Devolver eventos de un único stream, ordenados de forma ascendente y con versiones consecutivas.
- Realizar un único append atómico por llamada.
- Comparar la versión actual con `expectedVersion` dentro de la misma operación atómica que persiste los eventos.
- Lanzar `EventStoreConcurrencyException<TId>` si difieren, completando `ActualVersion` con la versión observada.
- No persistir ningún evento de esa llamada en caso de conflicto, cancelación o error.
- Propagar cancelación mediante `OperationCanceledException` y no aceptar los cambios del agregado.

El Core debe lanzar `ArgumentNullException` para dependencias, eventos, lista de eventos o factory nulos; `ArgumentOutOfRangeException` para versiones negativas; y `InvalidOperationException` para ciclos de vida inválidos, identificadores incompatibles o historial fuera de orden. Estas excepciones son fallos de uso/contrato; los errores de red, base de datos y mapeo pertenecen al adaptador y se propagan sin ser reinterpretados por Core.

## Riesgos y límites conocidos

- Un payload `IDomainEvent` tipado obliga al adaptador a contar con una estrategia de reconstrucción. Esa estrategia se mantiene fuera de v1; el `EventsMapper` actual es específico de `Account` y no se mueve al Core.
- La atomicidad real depende del proveedor. El store en memoria prueba la semántica; el futuro adaptador EF/PostgreSQL debe sostenerla con una transacción y una restricción única de stream/versión o mecanismo equivalente.
- La aceptación tras append confirma sólo persistencia del stream. No confirma publicación en RabbitMQ; resolver esa brecha requiere un outbox, explícitamente fuera de alcance.
- No hay upcasting ni metadata de esquema. La evolución de eventos persistidos deberá ser un requisito futuro antes de soportar cambios incompatibles de payload.
- La v1 no incluye snapshots; streams extensos se reproducen completos.

## Plan de verificación

Las pruebas unitarias de Core deben usar un agregado de prueba y un `IEventStore<TId>` in-memory. No requieren PostgreSQL ni RabbitMQ.

- `Raise` aplica el evento, lo deja pendiente y avanza la versión.
- `Replay` aplica eventos en orden, conserva su estado final, fija versiones y no crea pendientes.
- `Replay` rechaza identificadores distintos, primera versión distinta de 1, huecos y orden no ascendente.
- `SaveAsync` usa `PersistedVersion` como `expectedVersion`, asigna versiones consecutivas en el in-memory store y limpia pendientes tras éxito.
- `SaveAsync` sin pendientes no invoca `AppendAsync`.
- Dos agregados cargados en la misma versión producen éxito para el primero y `EventStoreConcurrencyException<TId>` para el segundo; el segundo conserva sus pendientes.
- Una excepción no relacionada del store también preserva pendientes y `PersistedVersion`.
- Las pruebas existentes de `Account` continúan verdes sin cambios. Las pruebas propias del Core usan solamente un dominio y un store in-memory de prueba; la adaptación de `Account` queda como siguiente incremento.

Al implementar, ejecutar al menos `dotnet test EjemploEventSourcing.sln`, `dotnet build EjemploEventSourcing.sln` y `git diff --check`. Los smoke tests con PostgreSQL/RabbitMQ no son criterio de esta extracción hasta que exista el adaptador correspondiente.
