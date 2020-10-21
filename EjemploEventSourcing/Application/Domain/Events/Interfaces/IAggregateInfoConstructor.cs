using System.Collections.Generic;

namespace EjemploEventSourcing.Application.Domain.Events.Interfaces
{
    public interface IAggregateInfoConstructor
    {
        string AggregateId { get; set; }
        int AggregateBaseVersion { get; set; }
        IEnumerable<IEvent> Events { get; set; }
    }
}