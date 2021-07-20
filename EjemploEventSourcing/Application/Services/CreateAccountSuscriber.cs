using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.IPresenters;
using EjemploEventSourcing.Application.services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EjemploEventSourcing.Application.Services
{
    public class CreateAccountSuscriber : IDomainEventsSuscriber
    {
        private readonly IEventStoreService _service;
        private readonly IAccountCreatedPresenter _presenter;

        public CreateAccountSuscriber(
            IEventStoreService service,
            IAccountCreatedPresenter presenter)
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
                _presenter.PublishAccountCreated(dto.AggregateId, dto.AggregateData);
            }
            catch (Exception ex)
            {
                _presenter.PublishErrorCreatingAccount(ex.Message);
                throw;
            }
        }

        public IEnumerable<EventTypes> SuscribeTo()
        {
            var suscribeList = new List<EventTypes>
            {
                EventTypes.AccountCreated
            };
            return suscribeList;
        }
    }
}
