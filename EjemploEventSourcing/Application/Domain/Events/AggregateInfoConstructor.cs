using System;
using System.Collections.Generic;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Domain.Events
{
    public class AggregateInfoConstructor : IAggregateInfoConstructor
    {
        public string AggregateId { get; set; }
        public int AggregateBaseVersion { get; set; }
        public IEnumerable<IEvent> Events { get; set; }
    }
}
