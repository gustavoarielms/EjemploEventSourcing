using System;
using System.Linq;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Infraestructura.Services;
using EjemploEventSourcing.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EjemploEventSourcing.Infraestructura.postgres
{
    public class GetAccountByIdDAO : IGetAccountByIdGateway
    {
        private readonly EventStoreDBContext _context;

        public GetAccountByIdDAO(EventStoreDBContext context)
        {
            _context = context;
        }

        public async Task<IAggregateInfoConstructor> GetAccountById(string accountId)
        {
            //if(!_context.Aggregates.Any())
            //    throw new InvalidOperationException();

            var aggregate = await _context.Aggregates.FindAsync(accountId);
            
            if (aggregate == null)
                throw new InvalidOperationException();

            var events = await _context.Events.Where(x => x.AggregateId == accountId).OrderBy(x => x.AggregateVersion).ToListAsync();
            return new AggregateInfoConstructor
            {
                AggregateId = aggregate.Id,
                AggregateBaseVersion = aggregate.LastVersion,
                Events = EventsMapper.EventsMapperRepositoryToIEvents(events)
            };
        }
    }
}
