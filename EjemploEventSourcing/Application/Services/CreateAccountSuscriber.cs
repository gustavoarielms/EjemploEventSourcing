using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.Domain.Events.Services;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Infraestructura.services;
using EjemploEventSourcing.Repositorios;

namespace EjemploEventSourcing.Application.Services
{
    public class CreateAccountSuscriber : IDomainEventsSuscriber
    {
        private readonly ICreateAccountGateway _gateway;
        private readonly IEventStoreService _service;
        private readonly EventStoreDBContext _context;

        public CreateAccountSuscriber(ICreateAccountGateway gateway, IEventStoreService service, EventStoreDBContext context)
        {
            _context = context;
            _service = service;
            _gateway = gateway;
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
            suscribeList.Add(EventTypes.AccountCreated);
            return suscribeList;
        }
    }
}
