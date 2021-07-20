using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AllEventsConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var rabbitFactory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "nimda",
                VirtualHost = "example-vhost",
                HostName = "localhost"
            };

            var httpClient = new HttpClient();

            using var connection = rabbitFactory.CreateConnection();
            using var channel = connection.CreateModel();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
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
            channel.BasicConsume(queue: "Events",
                                 autoAck: true,
                                 consumer: consumer);

            Console.ReadLine();
        }
    }
}
