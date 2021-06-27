using System;
using System.Linq;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Infraestructura.Services;
using EjemploEventSourcing.Repositorios;

namespace EjemploEventSourcing.Infraestructura.postgres
{
    public class GetAccountByIdDAO : IGetAccountByIdGateway
    {
        private readonly EventStoreDBContext _context;

        public GetAccountByIdDAO(EventStoreDBContext context)
        {
            _context = context;
        }

        IAggregateInfoConstructor IGetAccountByIdGateway.GetAccountById(string accountId)
        {
            if(!_context.Aggregates.Any())
                throw new InvalidOperationException();

            var aggregate = _context.Aggregates.ToList().DefaultIfEmpty(null).SingleOrDefault(b => b.Id == accountId);
            
            if (aggregate == null)
                throw new InvalidOperationException();

            var events = _context.Events.ToList().Where(x => x.AggregateId == accountId).OrderBy(x => x.AggregateVersion);
            return new AggregateInfoConstructor
            {
                AggregateId = aggregate.Id,
                AggregateBaseVersion = aggregate.LastVersion,
                Events = EventsMapper.EventsMapperRepositoryToIEvents(events)
            };
        }
    }
}
