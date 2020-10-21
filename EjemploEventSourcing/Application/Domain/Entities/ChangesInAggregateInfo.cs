using System;
using System.Collections.Generic;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Domain.Entities
{
    public class ChangesInAggregateInfo : IChangesInAggregateInfo
    {
        public IAggregateInfo AggregateInfo { get; set; }
        public IEnumerable<IEvent> Events { get; set; }
    }
}
