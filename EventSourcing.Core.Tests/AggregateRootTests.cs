using Xunit;

namespace EventSourcing.Core.Tests;

public class AggregateRootTests
{
    [Fact]
    public void Raise_AppliesAndRecordsTheEventAndAdvancesCurrentVersion()
    {
        var aggregate = CounterAggregate.Create("counter-1");

        aggregate.Increment(3);

        var @event = Assert.IsType<CounterIncremented>(Assert.Single(aggregate.PendingEvents));
        Assert.Equal(3, @event.Amount);
        Assert.Equal(3, aggregate.Value);
        Assert.Equal(1, aggregate.Version);
        Assert.Equal(0, aggregate.PersistedVersion);
    }

    [Fact]
    public void Raise_WhenApplyFails_DoesNotRecordTheEventOrAdvanceVersion()
    {
        var aggregate = CounterAggregate.Create("counter-1");

        var exception = Assert.Throws<InvalidOperationException>(aggregate.FailOnApply);

        Assert.Equal("Apply failed.", exception.Message);
        Assert.Empty(aggregate.PendingEvents);
        Assert.Equal(0, aggregate.Version);
        Assert.Equal(0, aggregate.PersistedVersion);
    }

    [Fact]
    public void Raise_RejectsNullEventsWithoutChangingTheAggregate()
    {
        var aggregate = CounterAggregate.Create("counter-1");

        Assert.Throws<ArgumentNullException>(aggregate.RaiseNull);

        Assert.Empty(aggregate.PendingEvents);
        Assert.Equal(0, aggregate.Version);
        Assert.Equal(0, aggregate.PersistedVersion);
    }
}
