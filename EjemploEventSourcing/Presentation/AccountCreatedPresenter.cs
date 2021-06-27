using EjemploEventSourcing.Application.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace EjemploEventSourcing.Presentation
{
    public class AccountCreatedPresenter : IAccountCreatedPresenter

    {
        private readonly IConnectionFactory _rabbitFactory;

        public AccountCreatedPresenter(IConnectionFactory rabbitFactory)
        {
            _rabbitFactory = rabbitFactory;
        }

        public void PublishAccountCreated(string id)
        {
            using var connection = _rabbitFactory.CreateConnection();
            using var channel = connection.CreateModel();
            var body = Encoding.UTF8.GetBytes(id);

            channel.BasicPublish(exchange: "example-exchange", routingKey: "event.AccountCreated", basicProperties: null, body: body);
        }

        public void PublishErrorCreatingAccount(string errorMessage)
        {
            using var connection = _rabbitFactory.CreateConnection();
            using var channel = connection.CreateModel();
            var body = Encoding.UTF8.GetBytes(errorMessage);

            channel.BasicPublish(exchange: "example-exchange", routingKey: "error.AccountCreated", basicProperties: null, body: body);
        }
    }
}
