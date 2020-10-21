using System.Collections.Generic;

namespace EjemploEventSourcing.Application.Domain.Events.Interfaces
{
    public  interface IChangesInAggregateInfo
    {
        IAggregateInfo AggregateInfo { get; set; }
        IEnumerable<IEvent> Events { get; set; }
    }
}