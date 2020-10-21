using System;
using System.Text.Json;
using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.DTO;

namespace EjemploEventSourcing.Application.Services
{
    public static class EventStoreMapper
    {
        public static EventStoredDTO EventMapper(IAggregateInfo aggregateInfo, int eventVersion, IEvent e)
        {
            return new EventStoredDTO
            {
                AggregateId = aggregateInfo.AggregateId,
                AggregateActualVersion = aggregateInfo.AggregateActualVersion,
                AggregateBaseVersion = aggregateInfo.AggregateBaseVersion,
                AggregateType = aggregateInfo.AggregateType,
                AggregateData = JsonSerializer.Serialize(e.GetData()),
                CreationDate = e.GetDateItHappened(),
                EventType = (int)e.GetEventType(),
                EventVersion = eventVersion
            };
      }
}
}
