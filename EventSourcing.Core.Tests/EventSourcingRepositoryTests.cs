using EventSourcing.Core;
using Xunit;

namespace EventSourcing.Core.Tests;

public class EventSourcingRepositoryTests
{
    [Fact]
    public async Task LoadAsync_WhenStreamDoesNotExist_ReturnsNullWithoutCreatingAggregate()
    {
        var store = new InMemoryEventStore<string>();
        var factoryCalls = 0;
        var repository = new EventSourcingRepository<CounterAggregate, string>(
            store,
            () =>
            {
                factoryCalls++;
                return new CounterAggregate();
            });

        var aggregate = await repository.LoadAsync("missing");

        Assert.Null(aggregate);
        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public async Task LoadAsync_ReplaysHistoryInOrderAndLeavesNoPendingEvents()
    {
        var store = new InMemoryEventStore<string>();
        store.Seed(
            "counter-1",
            Envelope("counter-1", 1, 5),
            Envelope("counter-1", 2, -2),
            Envelope("counter-1", 3, 4));
        var repository = Repository(store);

        var aggregate = await repository.LoadAsync("counter-1");

        Assert.NotNull(aggregate);
        Assert.Equal("counter-1", aggregate.Id);
        Assert.Equal(7, aggregate.Value);
        Assert.Equal([5, -2, 4], aggregate.AppliedAmounts);
        Assert.Equal(3, aggregate.Version);
        Assert.Equal(3, aggregate.PersistedVersion);
        Assert.Empty(aggregate.PendingEvents);
    }

    public static TheoryData<EventEnvelope<string>[]> InvalidHistories => new()
    {
        new[] { Envelope("other", 1, 1) },
        new[] { Envelope("counter-1", 2, 1) },
        new[] { Envelope("counter-1", 1, 1), Envelope("counter-1", 3, 1) },
        new[] { Envelope("counter-1", 1, 1), Envelope("counter-1", 1, 1) }
    };

    [Theory]
    [MemberData(nameof(InvalidHistories))]
    public async Task LoadAsync_RejectsAnInvalidStream(EventEnvelope<string>[] history)
    {
        var store = new InMemoryEventStore<string>();
        store.Seed("counter-1", history);
        var repository = Repository(store);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.LoadAsync("counter-1"));
    }

    [Fact]
    public async Task LoadAsync_WhenHistoryStructureIsInvalid_DoesNotApplyOrMutateTheAggregate()
    {
        var store = new InMemoryEventStore<string>();
        store.Seed("counter-1", Envelope("counter-1", 2, 5));
        CounterAggregate? createdAggregate = null;
        var repository = new EventSourcingRepository<CounterAggregate, string>(
            store,
            () => createdAggregate = new CounterAggregate());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.LoadAsync("counter-1"));

        Assert.NotNull(createdAggregate);
        Assert.Equal(0, createdAggregate.Value);
        Assert.Empty(createdAggregate.AppliedAmounts);
        Assert.Equal(0, createdAggregate.Version);
        Assert.Equal(0, createdAggregate.PersistedVersion);
        Assert.Empty(createdAggregate.PendingEvents);
    }

    [Fact]
    public async Task SaveAsync_AppendsUsingPersistedVersionThenAcceptsChanges()
    {
        var store = new InMemoryEventStore<string>();
        store.Seed("counter-1", Envelope("counter-1", 1, 5));
        var repository = Repository(store);
        var aggregate = await repository.LoadAsync("counter-1");
        Assert.NotNull(aggregate);
        aggregate.Increment(2);
        aggregate.Increment(3);

        await repository.SaveAsync(aggregate);

        Assert.Equal(1, store.AppendCalls);
        Assert.Equal(1, store.LastExpectedVersion);
        Assert.Equal(3, aggregate.Version);
        Assert.Equal(3, aggregate.PersistedVersion);
        Assert.Empty(aggregate.PendingEvents);

        var persisted = await store.ReadAsync("counter-1");
        Assert.Equal([1L, 2L, 3L], persisted.Select(x => x.Version));
        Assert.Equal([5, 2, 3], persisted.Select(x => Assert.IsType<CounterIncremented>(x.Event).Amount));
    }

    [Fact]
    public async Task SaveAsync_WithNoPendingEvents_DoesNotCallAppend()
    {
        var store = new InMemoryEventStore<string>();
        var repository = Repository(store);
        var aggregate = CounterAggregate.Create("counter-1");

        await repository.SaveAsync(aggregate);

        Assert.Equal(0, store.AppendCalls);
        Assert.Equal(0, aggregate.Version);
        Assert.Equal(0, aggregate.PersistedVersion);
    }

    [Fact]
    public async Task SaveAsync_WhenConcurrentWriteWins_PreservesPendingEventsAndPersistedVersion()
    {
        var store = new InMemoryEventStore<string>();
        store.Seed("counter-1", Envelope("counter-1", 1, 5));
        var repository = Repository(store);
        var first = await repository.LoadAsync("counter-1");
        var second = await repository.LoadAsync("counter-1");
        Assert.NotNull(first);
        Assert.NotNull(second);
        first.Increment(2);
        second.Increment(3);
        await repository.SaveAsync(first);

        var exception = await Assert.ThrowsAsync<EventStoreConcurrencyException<string>>(
            () => repository.SaveAsync(second));

        Assert.Equal("counter-1", exception.AggregateId);
        Assert.Equal(1, exception.ExpectedVersion);
        Assert.Equal(2, exception.ActualVersion);
        Assert.Equal(2, second.Version);
        Assert.Equal(1, second.PersistedVersion);
        Assert.IsType<CounterIncremented>(Assert.Single(second.PendingEvents));

        var persisted = await store.ReadAsync("counter-1");
        Assert.Equal(2, persisted.Count);
        Assert.Equal(2, Assert.IsType<CounterIncremented>(persisted[1].Event).Amount);
    }

    [Fact]
    public async Task SaveAsync_WhenAppendFailsAfterStaging_PreservesPendingEventsAndPersistsNothing()
    {
        var store = new InMemoryEventStore<string>
        {
            AppendFailure = new IOException("Store failed.")
        };
        var repository = Repository(store);
        var aggregate = CounterAggregate.Create("counter-1");
        aggregate.Increment(2);
        aggregate.Increment(3);

        var exception = await Assert.ThrowsAsync<IOException>(
            () => repository.SaveAsync(aggregate));

        Assert.Equal("Store failed.", exception.Message);
        Assert.Equal(2, aggregate.Version);
        Assert.Equal(0, aggregate.PersistedVersion);
        Assert.Equal(2, aggregate.PendingEvents.Count);
        Assert.Empty(await store.ReadAsync("counter-1"));
    }

    private static EventSourcingRepository<CounterAggregate, string> Repository(
        IEventStore<string> store)
    {
        return new EventSourcingRepository<CounterAggregate, string>(
            store,
            () => new CounterAggregate());
    }

    private static EventEnvelope<string> Envelope(string aggregateId, long version, int amount)
    {
        return new EventEnvelope<string>(
            aggregateId,
            version,
            new CounterIncremented(amount));
    }
}
