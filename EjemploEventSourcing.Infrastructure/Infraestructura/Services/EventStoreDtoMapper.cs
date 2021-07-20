using System;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.DTO;
using EjemploEventSourcing.Infrastructure.Repositorios;

namespace EjemploEventSourcing.Infrastructure.services
{
    public static class EventStoreDtoMapper
    {
        public static Event MapperFromEventToEventStoreDTO(EventStoredDTO dto) {
            var e = new Event()
            {
                AggregateId = dto.AggregateId,
                AggregateData = dto.AggregateData,
                DateTimeCreate = dto.CreationDate,
                MetaData = "",
                EventType = (EventTypes) dto.EventType,
            };
            return e;
        }
    }
}
