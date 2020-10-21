namespace EjemploEventSourcing.Application.Domain.Events.Interfaces
{
    public interface IInitAggregate
    {
        void BuildAggregate(IAggregateInfoConstructor data);
    }
}