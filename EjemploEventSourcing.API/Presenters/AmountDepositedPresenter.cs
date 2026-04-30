using EjemploEventSourcing.Application.DTO;
using EjemploEventSourcing.Application.IPresenters;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EjemploEventSourcing.Presenters
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
            PublishAmountDepositedAsync(aggregateId, aggregateData).GetAwaiter().GetResult();
        }

        private async Task PublishAmountDepositedAsync(string aggregateId, string aggregateData)
        {
            await using var connection = await _rabbitFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            var response = new ResponseDTO
            {
                AggregateId = aggregateId,
                AggregateData = aggregateData
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await channel.BasicPublishAsync(exchange: "example-exchange", routingKey: "event.AmountDeposited", body: body);
        }

        public void PublishErrorDepositingAmount(string errorMessage)
        {
            PublishErrorDepositingAmountAsync(errorMessage).GetAwaiter().GetResult();
        }

        private async Task PublishErrorDepositingAmountAsync(string errorMessage)
        {
            await using var connection = await _rabbitFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            var body = Encoding.UTF8.GetBytes(errorMessage);

            await channel.BasicPublishAsync(exchange: "example-exchange", routingKey: "error.AmountDeposited", body: body);
        }
    }
}
