using EjemploEventSourcing.Application.DTO;
using EjemploEventSourcing.Application.IPresenters;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            PublishAccountCreatedAsync(aggregateId, aggregateData).GetAwaiter().GetResult();
        }

        private async Task PublishAccountCreatedAsync(string aggregateId, string aggregateData)
        {
            await using var connection = await _rabbitFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            var response = new ResponseDTO
            {
                AggregateId = aggregateId,
                AggregateData = aggregateData
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await channel.BasicPublishAsync(exchange: "example-exchange", routingKey: "event.AccountCreated", body: body);
        }

        public void PublishErrorCreatingAccount(string errorMessage)
        {
            PublishErrorCreatingAccountAsync(errorMessage).GetAwaiter().GetResult();
        }

        private async Task PublishErrorCreatingAccountAsync(string errorMessage)
        {
            await using var connection = await _rabbitFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            var body = Encoding.UTF8.GetBytes(errorMessage);

            await channel.BasicPublishAsync(exchange: "example-exchange", routingKey: "error.AccountCreated", body: body);
        }
    }
}
