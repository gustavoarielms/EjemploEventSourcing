using System;
namespace EjemploEventSourcing.Application.DTO
{
    public class EventStoredDTO
    {
        public DateTime CreationDate { get; set; }
        public int AggregateBaseVersion { get; set; }
        public int AggregateActualVersion{ get; set; }
        public string AggregateId{ get; set; }
        public string AggregateData { get; set; }
        public string AggregateType { get; set; }
        public int EventType { get; set; }
        public int EventVersion { get; set; }
    }
}
