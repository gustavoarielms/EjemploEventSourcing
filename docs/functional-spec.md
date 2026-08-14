# Especificación funcional: biblioteca de Event Sourcing v1

## Estado y propósito

Este documento define la primera versión de una biblioteca reutilizable de Event Sourcing extraída gradualmente de este ejemplo. La biblioteca será una dependencia local .NET 10 en esta entrega; no se publica en NuGet remoto.

Su objetivo es resolver el ciclo mínimo y reusable de un agregado orientado a eventos: registrar eventos de dominio, reconstruir estado desde su historial y persistir cambios con concurrencia optimista. En esta entrega el Core funciona de manera independiente; `Account` permanece como ejemplo legacy y su migración como consumidor de la biblioteca se difiere a un siguiente incremento.

## Alcance

La v1 debe proporcionar:

- Un contrato tipado de evento de dominio sin datos de negocio en el núcleo.
- Una raíz de agregado genérica por tipo de identificador, con historial reproducible, versión actual y eventos pendientes.
- Un contrato de lectura y append atómico para un stream de agregado.
- Un repositorio genérico que cargue y guarde agregados usando ese contrato.
- Concurrencia optimista mediante la versión esperada del stream.
- La regla de confirmar/limpiar eventos pendientes sólo luego de un append exitoso.
- Pruebas de la biblioteca con un agregado de prueba y un store en memoria.

## Fuera de alcance

No forma parte de esta versión:

- Reglas, eventos o datos del dominio `Account` u otro dominio consumidor.
- RabbitMQ, publicación de eventos, suscriptores, outbox o integración externa.
- PostgreSQL, EF Core, migraciones, transacciones de proveedor o cualquier adaptador de persistencia concreto.
- JSON, serialización/deserialización, nombres globales de eventos, `enum EventTypes`, registro de tipos, upcasting o evolución de esquemas.
- Snapshots, proyecciones, consultas, sagas, CQRS, reintentos automáticos, idempotencia distribuida o multi-stream transactions.
- Login, APIs HTTP o configuración de DI.
- Empaquetado o publicación remota en NuGet.

## Actores y casos de uso

### Consumidor de la biblioteca

1. Define un evento de dominio que implementa `IDomainEvent` y un agregado que hereda de `AggregateRoot<TId>`.
2. El agregado ejecuta una operación de negocio propia y registra uno o más eventos pendientes.
3. El consumidor solicita al repositorio guardar el agregado.
4. El repositorio solicita un append atómico al store con la versión persistida del agregado como `expectedVersion`.
5. Si el append tiene éxito, el agregado acepta los eventos y no conserva pendientes. Si falla, conserva su estado y sus eventos pendientes para que el consumidor decida qué hacer.

### Reconstrucción de un agregado

1. El consumidor pide un agregado por identificador al repositorio.
2. El store devuelve los eventos persistidos de ese stream, en orden ascendente de versión.
3. El repositorio crea el agregado y reproduce ese historial.
4. El agregado queda en la versión del último evento, sin eventos pendientes y listo para ejecutar nuevas operaciones.

### Conflicto de escritura concurrente

1. Dos procesos cargan el mismo agregado en la misma versión.
2. El primero guarda y el store avanza el stream.
3. El segundo intenta guardar con su versión previa.
4. El store rechaza el append con un conflicto de concurrencia; el segundo agregado no acepta ni pierde sus eventos pendientes.

## Reglas e invariantes

- Un stream se identifica por el `TId` del agregado y usa versiones positivas, contiguas y crecientes desde 1.
- La versión de un agregado nuevo sin historial es 0.
- La versión esperada para guardar es exactamente la versión persistida del agregado antes de sus cambios pendientes.
- Cada evento pendiente incrementa en uno la versión en memoria.
- La reproducción aplica eventos en el orden de su versión, no los vuelve a registrar como pendientes y no ejecuta lógica de publicación.
- El agregado y todos los envelopes de un historial deben referirse al mismo identificador.
- La biblioteca trata el payload como `IDomainEvent` tipado. No lo convierte a `object`, texto, JSON ni a un tipo de evento global.
- El store debe hacer que el append completo sea atómico: o persiste todos los eventos con versiones consecutivas, o no persiste ninguno.
- `AcceptChanges` sólo puede ocurrir después de que `AppendAsync` finalice correctamente. Un conflicto o cualquier otra excepción deja pendientes intactos.
- Guardar un agregado sin eventos pendientes no escribe ni modifica la versión del stream.

## Criterios de aceptación

- Existe una biblioteca Core BCL-only con los contratos y comportamientos definidos en la especificación técnica.
- Un agregado de prueba puede registrar eventos, exponerlos como pendientes y reflejar su estado mediante su propio `Apply`.
- Un agregado de prueba puede reconstruirse desde un historial ordenado y terminar sin cambios pendientes.
- El store en memoria usado por pruebas rechaza un append cuando `expectedVersion` no coincide y no persiste eventos parciales.
- Después de un guardado exitoso, el agregado no tiene pendientes y su versión persistida coincide con la del stream.
- Después de un conflicto o error de append, el agregado conserva los eventos pendientes y su base de persistencia no se adelanta.
- Las pruebas cubren reproducción, orden inválido, versiones, eventos pendientes, éxito de aceptación y conflicto de concurrencia.
- La solución y `Account` continúan compilando sin cambios de comportamiento; `Account` todavía no usa los contratos del Core y los adaptadores existentes de EF/RabbitMQ no se incorporan a la biblioteca.

## Compatibilidad y migración futura del ejemplo

Esta entrega crea y valida el Core standalone y lo registra en la solución. `Account` y el proyecto Application continúan sin depender del Core hasta abordar su migración completa. Esto evita introducir una referencia sin uso, una compatibilidad artificial entre `IEvent`/`EventTypes` y `IDomainEvent`, o modificar en el mismo corte los interactors, mappers y publicadores existentes.

La migración posterior debe ser incremental y preservar el comportamiento didáctico de `Account`:

1. Crear el proyecto Core .NET 10 sin dependencias externas y mover o reexpresar allí sólo las abstracciones genéricas.
2. Cambiar `Account` y sus eventos para implementar/extender los contratos del Core, conservando sus reglas de creación, depósito y saldo en el proyecto consumidor.
3. Sustituir el uso interno de `Aggregate`, `IEvent` y sus versiones por `AggregateRoot<string>` e `IDomainEvent` del Core.
4. Mantener `EventsMapper`, JSON y EF Core fuera del Core; un adaptador futuro será responsable de convertir entre `EventEnvelope<string>` y el formato existente de persistencia.
5. Reemplazar el flujo stateful `Save`/`Commit` de `EventStoreService` por un adaptador que cumpla `IEventStore<string>.AppendAsync` de modo atómico y use la versión esperada.
6. Conservar RabbitMQ como integración del ejemplo. Esta entrega no cambia su semántica ni la conecta al guardado genérico.

No se exige compatibilidad binaria con las clases actuales. En esta entrega la compatibilidad requerida es que el ejemplo legacy conserve su flujo observable sin modificaciones; cuando se adapte a los nuevos contratos, deberá preservar ese mismo comportamiento.
