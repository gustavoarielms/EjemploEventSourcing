namespace EventSourcing.Core;

public sealed record EventEnvelope<TId>(
    TId AggregateId,
    long Version,
    IDomainEvent Event);
