using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Infraestructura.services;
using EjemploEventSourcing.Presentation;

namespace EjemploEventSourcing.Application.Services
{
    public class DepositAmountSuscriber : IDomainEventsSuscriber
    {
        private readonly IEventStoreService _service;
        private readonly IAmountDepositedPresenter _presenter;

        public DepositAmountSuscriber(
            IEventStoreService service,
            IAmountDepositedPresenter presenter)
        {
            _service = service;
            _presenter = presenter;
        }

        public async Task ManageEvent(IAggregateInfo aggregateInfo, int eventVersion, IEvent e)
        {
            var dto = EventStoreMapper.EventMapper(aggregateInfo, eventVersion, e);
            try
            {
                await _service.Save(dto);
                _presenter.PublishAmountDeposited(dto.AggregateId, dto.AggregateData);
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
