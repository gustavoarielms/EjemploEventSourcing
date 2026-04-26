using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Application.Interactors.CreateAccount;
using EjemploEventSourcing.Application.Interactors.DepositAmount;
using Xunit;

namespace EjemploEventSourcing.Tests.Interactors;

public class AccountInteractorTests
{
    [Fact]
    public async Task CreateAccountInteractor_PublishesAccountCreatedChanges()
    {
        var publisher = new RecordingPublisher();
        var interactor = new CreateAccountInteractor(publisher);

        await interactor.Execute("account-1");

        var changes = Assert.Single(publisher.PublishedChanges);
        var @event = Assert.Single(changes.Events);
        var data = Assert.IsType<DataAccountCreatedEvent>(@event.GetData());

        Assert.Equal("account-1", changes.AggregateInfo.AggregateId);
        Assert.Equal(0, changes.AggregateInfo.AggregateBaseVersion);
        Assert.Equal(1, changes.AggregateInfo.AggregateActualVersion);
        Assert.Equal(EventTypes.AccountCreated, @event.GetEventType());
        Assert.Equal("account-1", data.AccountId);
        Assert.Equal(0m, data.Balance);
    }

    [Fact]
    public async Task DepositAmountInteractor_ReplaysAccountAndPublishesAmountDepositedChanges()
    {
        var gateway = new StubGetAccountByIdGateway(BuildConstructor(
            "account-1",
            2,
            AccountCreated("account-1"),
            AmountDeposited("account-1", 10m)));
        var publisher = new RecordingPublisher();
        var interactor = new DepositAmountInteractor(gateway, publisher);

        await interactor.Execute("account-1", 25m);

        var changes = Assert.Single(publisher.PublishedChanges);
        var @event = Assert.Single(changes.Events);
        var data = Assert.IsType<DataAmountDepositedEvent>(@event.GetData());

        Assert.Equal("account-1", gateway.RequestedAccountId);
        Assert.Equal("account-1", changes.AggregateInfo.AggregateId);
        Assert.Equal(2, changes.AggregateInfo.AggregateBaseVersion);
        Assert.Equal(3, changes.AggregateInfo.AggregateActualVersion);
        Assert.Equal(EventTypes.AmountDeposited, @event.GetEventType());
        Assert.Equal("account-1", data.AccountId);
        Assert.Equal(25m, data.AmountDeposited);
    }

    [Fact]
    public async Task DepositAmountInteractor_PropagatesGatewayErrors()
    {
        var expected = new InvalidOperationException("account not found");
        var gateway = new ThrowingGetAccountByIdGateway(expected);
        var publisher = new RecordingPublisher();
        var interactor = new DepositAmountInteractor(gateway, publisher);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => interactor.Execute("missing-account", 25m));

        Assert.Same(expected, actual);
        Assert.Empty(publisher.PublishedChanges);
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

    private sealed class RecordingPublisher : IDomainEventsPublisher
    {
        public IList<IChangesInAggregateInfo> PublishedChanges { get; } = new List<IChangesInAggregateInfo>();

        public Task PublishEvent(IAggregateInfo aggregateInfo, int eventVersion, IEvent e)
        {
            return Task.CompletedTask;
        }

        public Task PublishEvents(IChangesInAggregateInfo changes)
        {
            PublishedChanges.Add(changes);
            return Task.CompletedTask;
        }
    }

    private sealed class StubGetAccountByIdGateway : IGetAccountByIdGateway
    {
        private readonly IAggregateInfoConstructor _constructor;

        public StubGetAccountByIdGateway(IAggregateInfoConstructor constructor)
        {
            _constructor = constructor;
        }

        public string? RequestedAccountId { get; private set; }

        public Task<IAggregateInfoConstructor> GetAccountById(string accountId)
        {
            RequestedAccountId = accountId;
            return Task.FromResult(_constructor);
        }
    }

    private sealed class ThrowingGetAccountByIdGateway : IGetAccountByIdGateway
    {
        private readonly Exception _exception;

        public ThrowingGetAccountByIdGateway(Exception exception)
        {
            _exception = exception;
        }

        public Task<IAggregateInfoConstructor> GetAccountById(string accountId)
        {
            return Task.FromException<IAggregateInfoConstructor>(_exception);
        }
    }
}
