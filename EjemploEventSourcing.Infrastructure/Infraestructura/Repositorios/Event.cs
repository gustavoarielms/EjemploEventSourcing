using System;
using System.ComponentModel.DataAnnotations.Schema;
using EjemploEventSourcing.Application.Domain.Events;

namespace EjemploEventSourcing.Infrastructure.Repositorios
{
    public class Event
    {
        public DateTime DateTimeCreate { get; set; }

        [Column(TypeName = "jsonb")]
        public string MetaData { get; set; }

        public int AggregateVersion { get; set; } //version number is unique and sequential only within the context of a given aggregate

        public string AggregateId { get; set; }

        [Column(TypeName = "jsonb")]
        public string AggregateData { get; set; }

        public EventTypes EventType { get; set; }
    }
}
