using EventSourcing.Core;

namespace EventSourcing.Core.Tests;

internal sealed record CounterIncremented(int Amount) : IDomainEvent;

internal sealed record CounterApplyFailed : IDomainEvent;

internal sealed class CounterAggregate : AggregateRoot<string>
{
    private readonly List<int> _appliedAmounts = [];

    public int Value { get; private set; }

    public IReadOnlyList<int> AppliedAmounts => _appliedAmounts;

    public static CounterAggregate Create(string id)
    {
        var aggregate = new CounterAggregate();
        aggregate.Initialize(id);
        return aggregate;
    }

    public void Increment(int amount) => Raise(new CounterIncremented(amount));

    public void FailOnApply() => Raise(new CounterApplyFailed());

    public void RaiseNull() => Raise(null!);

    protected override void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case CounterIncremented incremented:
                Value += incremented.Amount;
                _appliedAmounts.Add(incremented.Amount);
                break;
            case CounterApplyFailed:
                throw new InvalidOperationException("Apply failed.");
            default:
                throw new InvalidOperationException("Unknown event.");
        }
    }
}
