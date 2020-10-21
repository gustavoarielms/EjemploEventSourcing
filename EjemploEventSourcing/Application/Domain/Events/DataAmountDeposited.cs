namespace EjemploEventSourcing.Application.Domain.Events
{
    public class DataAmountDepositedEvent
    {
        public string AccountId { get; set; }
        public decimal AmountDeposited { get; set; }
    }
}