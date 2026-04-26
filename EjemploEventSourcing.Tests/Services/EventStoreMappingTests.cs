using System.Text.Json;
using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.DTO;
using EjemploEventSourcing.Application.Services;
using EjemploEventSourcing.Infrastructure.Repositorios;
using EjemploEventSourcing.Infrastructure.services;
using EjemploEventSourcing.Infrastructure.Services;
using Xunit;

namespace EjemploEventSourcing.Tests.Services;

public class EventStoreMappingTests
{
    [Fact]
    public void EventMapper_MapsDomainEventToStoredDto()
    {
        var aggregateInfo = new AggregateInfo
        {
            AggregateId = "account-1",
            AggregateBaseVersion = 1,
            AggregateActualVersion = 2,
            AggregateType = nameof(Account)
        };
        var domainEvent = AmountDeposited("account-1", 25m);

        var dto = EventStoreMapper.EventMapper(aggregateInfo, 2, domainEvent);
        var data = JsonSerializer.Deserialize<DataAmountDepositedEvent>(dto.AggregateData);

        Assert.Equal("account-1", dto.AggregateId);
        Assert.Equal(1, dto.AggregateBaseVersion);
        Assert.Equal(2, dto.AggregateActualVersion);
        Assert.Equal(nameof(Account), dto.AggregateType);
        Assert.Equal((int)EventTypes.AmountDeposited, dto.EventType);
        Assert.Equal(2, dto.EventVersion);
        Assert.Equal(domainEvent.GetDateItHappened(), dto.CreationDate);
        Assert.NotNull(data);
        Assert.Equal("account-1", data.AccountId);
        Assert.Equal(25m, data.AmountDeposited);
    }

    [Fact]
    public void MapperFromEventToEventStoreDTO_MapsStoredDtoToRepositoryEvent()
    {
        var creationDate = new DateTime(2026, 4, 25, 12, 0, 0);
        var dto = new EventStoredDTO
        {
            AggregateId = "account-1",
            AggregateData = """{"AccountId":"account-1","Balance":0}""",
            CreationDate = creationDate,
            EventType = (int)EventTypes.AccountCreated,
            EventVersion = 1
        };

        var repositoryEvent = EventStoreDtoMapper.MapperFromEventToEventStoreDTO(dto);

        Assert.Equal("account-1", repositoryEvent.AggregateId);
        Assert.Equal(dto.AggregateData, repositoryEvent.AggregateData);
        Assert.Equal(creationDate, repositoryEvent.DateTimeCreate);
        Assert.Equal(EventTypes.AccountCreated, repositoryEvent.EventType);
        Assert.Equal(0, repositoryEvent.AggregateVersion);
        Assert.Equal(string.Empty, repositoryEvent.MetaData);
    }

    [Fact]
    public void EventMapperRepositoryToIEvent_MapsAccountCreatedRepositoryEventToDomainEvent()
    {
        var repositoryEvent = new Event
        {
            AggregateId = "account-1",
            AggregateData = """{"AccountId":"account-1","Balance":0}""",
            EventType = EventTypes.AccountCreated
        };

        var domainEvent = EventsMapper.EventMapperRepositoryToIEvent(repositoryEvent);
        var data = Assert.IsType<DataAccountCreatedEvent>(domainEvent.GetData());

        Assert.Equal(EventTypes.AccountCreated, domainEvent.GetEventType());
        Assert.Equal("account-1", data.AccountId);
        Assert.Equal(0m, data.Balance);
    }

    [Fact]
    public void EventMapperRepositoryToIEvent_MapsAmountDepositedRepositoryEventToDomainEvent()
    {
        var repositoryEvent = new Event
        {
            AggregateId = "account-1",
            AggregateData = """{"AccountId":"account-1","AmountDeposited":25}""",
            EventType = EventTypes.AmountDeposited
        };

        var domainEvent = EventsMapper.EventMapperRepositoryToIEvent(repositoryEvent);
        var data = Assert.IsType<DataAmountDepositedEvent>(domainEvent.GetData());

        Assert.Equal(EventTypes.AmountDeposited, domainEvent.GetEventType());
        Assert.Equal("account-1", data.AccountId);
        Assert.Equal(25m, data.AmountDeposited);
    }

    [Fact]
    public void EventsMapperRepositoryToIEvents_MapsRepositoryEventSequence()
    {
        var repositoryEvents = new[]
        {
            new Event
            {
                AggregateData = """{"AccountId":"account-1","Balance":0}""",
                EventType = EventTypes.AccountCreated
            },
            new Event
            {
                AggregateData = """{"AccountId":"account-1","AmountDeposited":25}""",
                EventType = EventTypes.AmountDeposited
            }
        };

        var domainEvents = EventsMapper.EventsMapperRepositoryToIEvents(repositoryEvents).ToList();

        Assert.Collection(
            domainEvents,
            e => Assert.Equal(EventTypes.AccountCreated, e.GetEventType()),
            e => Assert.Equal(EventTypes.AmountDeposited, e.GetEventType()));
    }

    [Fact]
    public void EventMapperRepositoryToIEvent_ThrowsForUnknownEventType()
    {
        var repositoryEvent = new Event
        {
            AggregateData = "{}",
            EventType = (EventTypes)999
        };

        Assert.Throws<InvalidOperationException>(() => EventsMapper.EventMapperRepositoryToIEvent(repositoryEvent));
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
