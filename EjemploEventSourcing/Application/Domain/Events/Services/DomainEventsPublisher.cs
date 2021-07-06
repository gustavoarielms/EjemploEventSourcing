using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Domain.Events.Services
{
    public class DomainEventsPublisher
    {
        private IList<IDomainEventsSuscriber> _suscribers;
        private static DomainEventsPublisher _instance;

        public DomainEventsPublisher()
        {
            _suscribers = Enumerable.Empty<IDomainEventsSuscriber>().ToList();
        }

        public static DomainEventsPublisher GetInstancia()
        {
            if (_instance != null)
                return _instance;

            _instance = new DomainEventsPublisher();
            return _instance;
        }

        public void RegisterSuscriber(IDomainEventsSuscriber suscriber)
        {
            _suscribers.Add(suscriber);
        }

        public void RemoveSuscriber(IDomainEventsSuscriber suscriber)
        {
            _suscribers.Remove(suscriber);
        }

        public void ResetSuscribers()
        {
            _suscribers = Enumerable.Empty<IDomainEventsSuscriber>().ToList();
        }

        public void PublishEvent(IAggregateInfo aggregateInfo, int eventVersion, IEvent e)
        {
            var suscribers = _suscribers.Where(x => x.SuscribeTo().Any(y => y == e.GetEventType())).ToList();


            foreach (var suscriber in suscribers) 
            {
                suscriber.ManageEvent(aggregateInfo, eventVersion, e);
            }
        }

        public void PublishEvents(IChangesInAggregateInfo changes)
        {
            var increment = 1;
            foreach (var e in changes.Events)
            {
                var eventVersion = changes.AggregateInfo.AggregateBaseVersion + increment;
                PublishEvent(changes.AggregateInfo, eventVersion, e);
                increment++;
            }
        }
    }
}
