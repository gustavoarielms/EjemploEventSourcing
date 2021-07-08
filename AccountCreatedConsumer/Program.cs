using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;

namespace AccountCreatedConsumer
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

            using var connection = rabbitFactory.CreateConnection();
            using var channel = connection.CreateModel();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var response = JsonSerializer.Deserialize<ResponseDTO>(message);
                Console.WriteLine($" [x] Received {response.AggregateId}, {response.AggregateData}");
            };
            channel.BasicConsume(queue: "AccountCreated",
                                 autoAck: false,
                                 consumer: consumer);

            Console.ReadLine();
        }
    }
}
