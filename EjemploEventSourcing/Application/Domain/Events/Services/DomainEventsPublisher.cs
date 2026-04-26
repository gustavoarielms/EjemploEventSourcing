using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Domain.Events.Services
{
    public class DomainEventsPublisher : IDomainEventsPublisher
    {
        private IList<IDomainEventsSuscriber> _suscribers;

        public DomainEventsPublisher(IEnumerable<IDomainEventsSuscriber> suscribers)
        {
            _suscribers = suscribers.ToList();
        }

        public async Task PublishEvent(IAggregateInfo aggregateInfo, int eventVersion, IEvent e)
        {
            var suscribers = _suscribers.Where(x => x.SuscribeTo().Any(y => y == e.GetEventType())).ToList();


            foreach (var suscriber in suscribers) 
            {
                await suscriber.ManageEvent(aggregateInfo, eventVersion, e);
            }
        }

        public async Task PublishEvents(IChangesInAggregateInfo changes)
        {
            var increment = 1;
            foreach (var e in changes.Events)
            {
                var eventVersion = changes.AggregateInfo.AggregateBaseVersion + increment;
                await PublishEvent(changes.AggregateInfo, eventVersion, e);
                increment++;
            }
        }
    }
}
