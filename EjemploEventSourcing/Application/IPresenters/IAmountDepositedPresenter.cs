namespace EjemploEventSourcing.Application.IPresenters
{
    public interface IAmountDepositedPresenter
    {
        void PublishAmountDeposited(string id, string aggregateData);
        void PublishErrorDepositingAmount(string message);
    }
}
