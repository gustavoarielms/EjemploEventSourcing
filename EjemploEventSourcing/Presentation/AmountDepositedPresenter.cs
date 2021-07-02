using EjemploEventSourcing.Application.Configuration;
using EjemploEventSourcing.Application.DTO;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EjemploEventSourcing.Presentation
{
    public class AmountDepositedPresenter : IAmountDepositedPresenter

    {
        private readonly IConnectionFactory _rabbitFactory;

        public AmountDepositedPresenter(IConnectionFactory rabbitFactory)
        {
            _rabbitFactory = rabbitFactory;
        }

        public void PublishAmountDeposited(string aggregateId, string aggregateData)
        {
            using var connection = _rabbitFactory.CreateConnection();
            using var channel = connection.CreateModel();
            var response = new ResponseDTO
            {
                AggregateId = aggregateId,
                AggregateData = aggregateData
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            channel.BasicPublish(exchange: "example-exchange", routingKey: "event.AmountDeposited", basicProperties: null, body: body);
        }

        public void PublishErrorDepositingAmount(string errorMessage)
        {
            using var connection = _rabbitFactory.CreateConnection();
            using var channel = connection.CreateModel();
            var body = Encoding.UTF8.GetBytes(errorMessage);

            channel.BasicPublish(exchange: "example-exchange", routingKey: "error.AmountDeposited", basicProperties: null, body: body);
        }
    }
}
