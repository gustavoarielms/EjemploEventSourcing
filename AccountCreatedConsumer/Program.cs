using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AccountCreatedConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rabbitFactory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "nimda",
                VirtualHost = "example-vhost",
                HostName = "localhost"
            };

            await using var connection = await rabbitFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var response = JsonSerializer.Deserialize<ResponseDTO>(message);
                Console.WriteLine($" [x] Received {response.AggregateId}, {response.AggregateData}");
                return Task.CompletedTask;
            };
            await channel.BasicConsumeAsync(queue: "AccountCreated",
                                            autoAck: false,
                                            consumer: consumer);

            Console.ReadLine();
        }
    }
}
