using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AllEventsConsumer
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

            var httpClient = new HttpClient();

            await using var connection = await rabbitFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var responseDTO = JsonSerializer.Deserialize<ResponseDTO>(message);
                var httpResponse = await httpClient.GetAsync($"https://localhost:5001/GetAccountById/{responseDTO.AggregateId}");

                if (httpResponse.IsSuccessStatusCode)
                {
                    var response = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($" [x] Received {response}");
                }

            };
            await channel.BasicConsumeAsync(queue: "Events",
                                            autoAck: true,
                                            consumer: consumer);

            Console.ReadLine();
        }
    }
}
