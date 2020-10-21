using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Infraestructura.services;

namespace EjemploEventSourcing.Application.Services
{
    public class DepositAmountSuscriber : IDomainEventsSuscriber
    {
        private readonly IEventStoreService _service;

        public DepositAmountSuscriber(IEventStoreService service)
        {
            _service = service;
        }

        public async Task ManageEvent(IAggregateInfo aggregateInfo, int eventVersion, IEvent e)
        {
            var dto = EventStoreMapper.EventMapper(aggregateInfo, eventVersion, e);
            try
            {
                await _service.Save(dto);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<EventTypes> SuscribeTo()
        {
            var suscribeList = new List<EventTypes>();
            suscribeList.Add(EventTypes.AmountDeposited);
            return suscribeList;
        }
    }
}
