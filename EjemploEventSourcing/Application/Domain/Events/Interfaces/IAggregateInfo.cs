namespace EjemploEventSourcing.Application.Domain.Events.Interfaces
{
    public interface IAggregateInfo
    {
        string AggregateId { get; set; }
        int AggregateBaseVersion { get; set; }
        int AggregateActualVersion { get; set; }
        string AggregateType { get; set; }
    }
}