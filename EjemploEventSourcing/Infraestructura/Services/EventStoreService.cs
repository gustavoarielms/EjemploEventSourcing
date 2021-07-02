using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.DTO;
using EjemploEventSourcing.Infraestructura.Repositorios;
using EjemploEventSourcing.Repositorios;

namespace EjemploEventSourcing.Infraestructura.services
{
    public class EventStoreService : IEventStoreService
    {
        private readonly IList<EventStoredDTO> _events;
        private string _aggregateId;
        private int _aggregateActualVersion = 0;
        private int _aggregateBaseVersion = 0;
        private string _aggregateType = null;

        private readonly EventStoreDBContext _context;

        public EventStoreService(EventStoreDBContext context)
        {
            _context = context;
            _events = Enumerable.Empty<EventStoredDTO>().ToList();
        }

        private void SetAggregateId(string aggregateId)
        {
            if (_aggregateId == null)
            {
                _aggregateId = aggregateId;
            }
        }

        public async Task Commit()
        {
            //using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (_context.Aggregates.Any())
                {
                    var aggregate = await _context.Aggregates.FindAsync(_aggregateId);

                    if (aggregate == null)
                    {
                        aggregate = new Aggregate
                        {
                            Id = _aggregateId,
                            Type = _aggregateType,
                            LastVersion = _aggregateActualVersion
                        };

                        _context.Aggregates.Add(aggregate);
                    }
                    else
                    {
                        aggregate.LastVersion = _aggregateActualVersion;
                    }

                    //await _context.SaveChangesAsync();

                    foreach (var e in _events)
                    {
                        var eventToSave = EventStoreDtoMapper.MapperFromEventToEventStoreDTO(e);
                        eventToSave.MetaData = eventToSave.AggregateData;
                        eventToSave.AggregateVersion = e.EventVersion;
                        _context.Events.Add(eventToSave);


                    }

                    await _context.SaveChangesAsync();
                    //await transaction.CommitAsync();
                }
            }
            catch (Exception)
            {
                //await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task Save(EventStoredDTO e)
        {
            SetAggregateId(e.AggregateId);
            _aggregateActualVersion = e.AggregateActualVersion;
            _aggregateBaseVersion = e.AggregateBaseVersion;
            _aggregateType = e.AggregateType;
            var index = e.EventVersion - _aggregateBaseVersion - 1;
            var differenceActualBase = _aggregateActualVersion - _aggregateBaseVersion;
            var aux = _events.DefaultIfEmpty(null).ElementAt(index);
            if (aux == null)
                _events.Insert(index, e);
            else
                aux = e;
            if (_events.Count() == differenceActualBase)
                await Commit();
        }
    }
}
