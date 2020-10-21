using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EjemploEventSourcing.Application.Domain.Events.Interfaces
{
    public interface IDomainEventsSuscriber
    {
        IEnumerable<EventTypes> SuscribeTo();
        Task ManageEvent(IAggregateInfo aggregateInfo, int eventVersion, IEvent e);
    }
}
