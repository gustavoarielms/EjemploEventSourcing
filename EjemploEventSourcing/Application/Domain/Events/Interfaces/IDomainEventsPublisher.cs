using System.Threading.Tasks;

namespace EjemploEventSourcing.Application.Domain.Events.Interfaces
{
    public interface IDomainEventsPublisher
    {
        Task PublishEvent(IAggregateInfo aggregateInfo, int eventVersion, IEvent e);
        Task PublishEvents(IChangesInAggregateInfo changes);
    }
}
