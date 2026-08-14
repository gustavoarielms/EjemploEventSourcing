using EventSourcing.Core;

namespace EventSourcing.Core.Tests;

internal sealed class InMemoryEventStore<TId> : IEventStore<TId>
    where TId : notnull
{
    private readonly Dictionary<TId, IReadOnlyList<EventEnvelope<TId>>> _streams = [];

    public int AppendCalls { get; private set; }

    public long? LastExpectedVersion { get; private set; }

    public Exception? AppendFailure { get; set; }

    public Task<IReadOnlyList<EventEnvelope<TId>>> ReadAsync(
        TId aggregateId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            _streams.TryGetValue(aggregateId, out var history)
                ? history
                : (IReadOnlyList<EventEnvelope<TId>>)Array.Empty<EventEnvelope<TId>>());
    }

    public Task AppendAsync(
        TId aggregateId,
        long expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedVersion);
        cancellationToken.ThrowIfCancellationRequested();

        AppendCalls++;
        LastExpectedVersion = expectedVersion;

        var existing = _streams.TryGetValue(aggregateId, out var history)
            ? history
            : Array.Empty<EventEnvelope<TId>>();
        var actualVersion = existing.Count;

        if (actualVersion != expectedVersion)
        {
            throw new EventStoreConcurrencyException<TId>(
                aggregateId,
                expectedVersion,
                actualVersion);
        }

        var staged = new EventEnvelope<TId>[events.Count];
        for (var index = 0; index < events.Count; index++)
        {
            var @event = events[index]
                ?? throw new ArgumentNullException(nameof(events), "Events cannot contain null values.");
            staged[index] = new EventEnvelope<TId>(
                aggregateId,
                expectedVersion + index + 1,
                @event);
        }

        if (AppendFailure is not null)
        {
            throw AppendFailure;
        }

        _streams[aggregateId] = [.. existing, .. staged];
        return Task.CompletedTask;
    }

    public void Seed(TId aggregateId, params EventEnvelope<TId>[] history)
    {
        _streams[aggregateId] = history;
    }
}
