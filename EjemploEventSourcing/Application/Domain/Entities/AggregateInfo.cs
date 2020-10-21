using System;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Domain.Entities
{
    public class AggregateInfo : IAggregateInfo
    {
        public string AggregateId { get; set; }
        public int AggregateBaseVersion { get; set; }
        public int AggregateActualVersion { get; set; }
        public string AggregateType { get; set; }
    }
}
