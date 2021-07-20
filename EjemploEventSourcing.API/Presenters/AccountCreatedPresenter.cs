using EjemploEventSourcing.Application.DTO;
using EjemploEventSourcing.Application.IPresenters;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EjemploEventSourcing.Presenters
{
    public class AccountCreatedPresenter : IAccountCreatedPresenter

    {
        private readonly IConnectionFactory _rabbitFactory;

        public AccountCreatedPresenter(IConnectionFactory rabbitFactory)
        {
            _rabbitFactory = rabbitFactory;
        }

        public void PublishAccountCreated(string aggregateId, string aggregateData)
        {
            using var connection = _rabbitFactory.CreateConnection();
            using var channel = connection.CreateModel();
            var response = new ResponseDTO
            {
                AggregateId = aggregateId,
                AggregateData = aggregateData
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
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
