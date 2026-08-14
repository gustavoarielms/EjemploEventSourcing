namespace EventSourcing.Core;

public sealed class EventStoreConcurrencyException<TId> : Exception
{
    public EventStoreConcurrencyException(
        TId aggregateId,
        long expectedVersion,
        long actualVersion)
        : base(
            $"Concurrency conflict for aggregate '{aggregateId}': " +
            $"expected version {expectedVersion}, actual version {actualVersion}.")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expectedVersion);
        ArgumentOutOfRangeException.ThrowIfNegative(actualVersion);

        AggregateId = aggregateId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public TId AggregateId { get; }

    public long ExpectedVersion { get; }

    public long ActualVersion { get; }
}
