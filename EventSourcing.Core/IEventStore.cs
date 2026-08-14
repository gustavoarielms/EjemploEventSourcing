namespace EventSourcing.Core;

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
