using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Infrastructure.Repositorios;

namespace EjemploEventSourcing.Infrastructure.Services
{
    public static class EventsMapper
    {
        public static IEnumerable<IEvent> EventsMapperRepositoryToIEvents(IEnumerable<Event> eventsRepository)
        {
            var iEvents = new List<IEvent>();
            foreach(var e in eventsRepository)
            {
                var iEvent = EventMapperRepositoryToIEvent(e);
                iEvents.Add(iEvent);
            }
            return iEvents;
        }

        public static IEvent EventMapperRepositoryToIEvent(Event e)
        {
            switch (e.EventType)
            {
                case EventTypes.AccountCreated:
                    {
                        return new AccountCreatedEvent(JsonSerializer.Deserialize<DataAccountCreatedEvent>(e.AggregateData));
                    }
                case EventTypes.AmountDeposited:
                    {
                        return new AmountDepositedEvent(JsonSerializer.Deserialize<DataAmountDepositedEvent>(e.AggregateData));
                    }
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
