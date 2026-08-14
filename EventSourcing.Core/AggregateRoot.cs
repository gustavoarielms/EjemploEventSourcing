using System.Collections.ObjectModel;

namespace EventSourcing.Core;

public abstract class AggregateRoot<TId>
{
    private readonly List<IDomainEvent> _pendingEvents = [];
    private readonly ReadOnlyCollection<IDomainEvent> _readOnlyPendingEvents;
    private bool _initialized;

    protected AggregateRoot()
    {
        _readOnlyPendingEvents = _pendingEvents.AsReadOnly();
    }

    public TId Id { get; private set; } = default!;

    public long Version { get; private set; }

    public long PersistedVersion { get; private set; }

    public IReadOnlyList<IDomainEvent> PendingEvents => _readOnlyPendingEvents;

    protected void Initialize(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (_initialized)
        {
            throw new InvalidOperationException("The aggregate is already initialized.");
        }

        Id = id;
        Version = 0;
        PersistedVersion = 0;
        _initialized = true;
    }

    protected void Raise(IDomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!_initialized)
        {
            throw new InvalidOperationException("The aggregate must be initialized before raising events.");
        }

        Apply(@event);
        _pendingEvents.Add(@event);
        Version++;
    }

    protected abstract void Apply(IDomainEvent @event);

    internal void Replay(IReadOnlyList<EventEnvelope<TId>> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        if (_initialized || _pendingEvents.Count > 0)
        {
            throw new InvalidOperationException("History can only be replayed on a new aggregate.");
        }

        if (history.Count == 0)
        {
            throw new InvalidOperationException("An empty history cannot initialize an aggregate.");
        }

        var firstEnvelope = history[0]
            ?? throw new InvalidOperationException("History cannot contain null envelopes.");
        var aggregateId = firstEnvelope.AggregateId;
        ArgumentNullException.ThrowIfNull(aggregateId);

        for (var index = 0; index < history.Count; index++)
        {
            var envelope = history[index]
                ?? throw new InvalidOperationException("History cannot contain null envelopes.");

            if (!EqualityComparer<TId>.Default.Equals(envelope.AggregateId, aggregateId))
            {
                throw new InvalidOperationException("History contains events for different aggregate identifiers.");
            }

            var expectedVersion = index + 1L;
            if (envelope.Version != expectedVersion)
            {
                throw new InvalidOperationException(
                    $"History version {envelope.Version} is invalid; expected {expectedVersion}.");
            }

            if (envelope.Event is null)
            {
                throw new ArgumentNullException(nameof(history), "History cannot contain null events.");
            }
        }

        foreach (var envelope in history)
        {
            Apply(envelope.Event);
        }

        Id = aggregateId;
        Version = history[^1].Version;
        PersistedVersion = Version;
        _initialized = true;
    }

    internal void AcceptChanges()
    {
        PersistedVersion = Version;
        _pendingEvents.Clear();
    }
}
