namespace EventSourcing.Core;

public sealed class EventSourcingRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
{
    private readonly IEventStore<TId> _eventStore;
    private readonly Func<TAggregate> _aggregateFactory;

    public EventSourcingRepository(
        IEventStore<TId> eventStore,
        Func<TAggregate> aggregateFactory)
    {
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(aggregateFactory);

        _eventStore = eventStore;
        _aggregateFactory = aggregateFactory;
    }

    public async Task<TAggregate?> LoadAsync(
        TId aggregateId,
        CancellationToken cancellationToken = default)
    {
        var history = await _eventStore.ReadAsync(aggregateId, cancellationToken);
        ArgumentNullException.ThrowIfNull(history);

        if (history.Count == 0)
        {
            return null;
        }

        if (history.Any(envelope =>
                envelope is null ||
                !EqualityComparer<TId>.Default.Equals(envelope.AggregateId, aggregateId)))
        {
            throw new InvalidOperationException(
                "The stored history does not match the requested aggregate identifier.");
        }

        var aggregate = _aggregateFactory()
            ?? throw new InvalidOperationException("The aggregate factory returned null.");
        aggregate.Replay(history);
        return aggregate;
    }

    public async Task SaveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (aggregate.PendingEvents.Count == 0)
        {
            return;
        }

        await _eventStore.AppendAsync(
            aggregate.Id,
            aggregate.PersistedVersion,
            aggregate.PendingEvents,
            cancellationToken);
        aggregate.AcceptChanges();
    }
}
