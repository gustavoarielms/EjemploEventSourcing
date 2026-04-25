using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using Xunit;

namespace EjemploEventSourcing.Tests.Domain;

public class AccountTests
{
    [Fact]
    public void CreateAccount_RecordsAccountCreatedEvent()
    {
        var account = Account.CreateAccount("account-1");

        var changes = account.GetChanges();
        var @event = Assert.Single(changes.Events);
        var data = Assert.IsType<DataAccountCreatedEvent>(@event.GetData());

        Assert.True(account.HasChanges());
        Assert.Equal("account-1", changes.AggregateInfo.AggregateId);
        Assert.Equal(0, changes.AggregateInfo.AggregateBaseVersion);
        Assert.Equal(1, changes.AggregateInfo.AggregateActualVersion);
        Assert.Equal(nameof(Account), changes.AggregateInfo.AggregateType);
        Assert.Equal(EventTypes.AccountCreated, @event.GetEventType());
        Assert.Equal("account-1", data.AccountId);
        Assert.Equal(0m, data.Balance);
    }

    [Fact]
    public void DepositAmount_RecordsAmountDepositedEventWithNextVersion()
    {
        var account = Account.CreateEmptyAccount("account-1");
        account.BuildAggregate(BuildConstructor("account-1", 1, AccountCreated("account-1")));

        account.DepositAmount(25m);

        var changes = account.GetChanges();
        var @event = Assert.Single(changes.Events);
        var data = Assert.IsType<DataAmountDepositedEvent>(@event.GetData());

        Assert.True(account.HasChanges());
        Assert.Equal(1, changes.AggregateInfo.AggregateBaseVersion);
        Assert.Equal(2, changes.AggregateInfo.AggregateActualVersion);
        Assert.Equal(EventTypes.AmountDeposited, @event.GetEventType());
        Assert.Equal("account-1", data.AccountId);
        Assert.Equal(25m, data.AmountDeposited);
    }

    [Fact]
    public void BuildAggregate_ReplaysStoredEvents()
    {
        var account = Account.CreateEmptyAccount("account-1");

        account.BuildAggregate(BuildConstructor(
            "account-1",
            3,
            AccountCreated("account-1"),
            AmountDeposited("account-1", 50m),
            AmountDeposited("account-1", 75m)));

        Assert.False(account.HasChanges());
        Assert.Equal("account-1", account.Id);
        Assert.Equal(125m, account.Balance);
    }

    [Fact]
    public void BuildAggregate_ThrowsWhenBaseVersionDoesNotMatchReplayedEvents()
    {
        var account = Account.CreateEmptyAccount("account-1");
        var constructor = BuildConstructor("account-1", 2, AccountCreated("account-1"));

        Assert.Throws<InvalidOperationException>(() => account.BuildAggregate(constructor));
    }

    private static AggregateInfoConstructor BuildConstructor(
        string aggregateId,
        int baseVersion,
        params IEvent[] events)
    {
        return new AggregateInfoConstructor
        {
            AggregateId = aggregateId,
            AggregateBaseVersion = baseVersion,
            Events = events
        };
    }

    private static AccountCreatedEvent AccountCreated(string accountId)
    {
        return new AccountCreatedEvent(new DataAccountCreatedEvent
        {
            AccountId = accountId,
            Balance = 0m
        });
    }

    private static AmountDepositedEvent AmountDeposited(string accountId, decimal amount)
    {
        return new AmountDepositedEvent(new DataAmountDepositedEvent
        {
            AccountId = accountId,
            AmountDeposited = amount
        });
    }
}
