namespace EjemploEventSourcing.Application.IPresenters
{
    public interface IAccountCreatedPresenter
    {
        void PublishAccountCreated(string id, string aggregateData);
        void PublishErrorCreatingAccount(string message);
    }
}
