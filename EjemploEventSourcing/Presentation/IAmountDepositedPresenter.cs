namespace EjemploEventSourcing.Presentation
{
    public interface IAmountDepositedPresenter
    {
        void PublishAmountDeposited(string id, string aggregateData);
        void PublishErrorDepositingAmount(string message);
    }
}
