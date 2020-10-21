namespace EjemploEventSourcing.Application.Domain.Events
{
    public class DataAccountCreatedEvent
    {
        public string AccountId { get; set; }
        public decimal Balance { get; set; }
    }
}