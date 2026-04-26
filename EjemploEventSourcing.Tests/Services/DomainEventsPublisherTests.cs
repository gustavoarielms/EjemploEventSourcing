using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.Domain.Events.Services;
using Xunit;

namespace EjemploEventSourcing.Tests.Services;

public class DomainEventsPublisherTests
{
    [Fact]
    public async Task PublishEvents_DispatchesMatchingSubscribers()
    {
        var accountCreatedSubscriber = new RecordingSubscriber(EventTypes.AccountCreated);
        var amountDepositedSubscriber = new RecordingSubscriber(EventTypes.AmountDeposited);
        var publisher = new DomainEventsPublisher(new IDomainEventsSuscriber[]
        {
            accountCreatedSubscriber,
            amountDepositedSubscriber
        });
        var changes = Account.CreateAccount("account-1").GetChanges();

        await publisher.PublishEvents(changes);

        var published = Assert.Single(accountCreatedSubscriber.PublishedEvents);
        Assert.Empty(amountDepositedSubscriber.PublishedEvents);
        Assert.Equal(EventTypes.AccountCreated, published.Event.GetEventType());
        Assert.Equal(1, published.EventVersion);
        Assert.Equal("account-1", published.AggregateInfo.AggregateId);
    }

    private sealed class RecordingSubscriber : IDomainEventsSuscriber
    {
        private readonly EventTypes _eventType;

        public RecordingSubscriber(EventTypes eventType)
        {
            _eventType = eventType;
        }

        public IList<PublishedEvent> PublishedEvents { get; } = new List<PublishedEvent>();

        public IEnumerable<EventTypes> SuscribeTo()
        {
            return new[] { _eventType };
        }

        public Task ManageEvent(IAggregateInfo aggregateInfo, int eventVersion, IEvent e)
        {
            PublishedEvents.Add(new PublishedEvent(aggregateInfo, eventVersion, e));
            return Task.CompletedTask;
        }
    }

    private sealed record PublishedEvent(
        IAggregateInfo AggregateInfo,
        int EventVersion,
        IEvent Event);
}
