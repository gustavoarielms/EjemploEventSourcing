namespace EjemploEventSourcing.Presentation
{
    public interface IAccountCreatedPresenter
    {
        void PublishAccountCreated(string id);
        void PublishErrorCreatingAccount(string message);
    }
}
