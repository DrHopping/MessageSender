using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using MessageSender.Models;
using MessageSender.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace MessageSender.Handlers
{
    public class KafkaConsumerEmailTelegramHandler : IHostedService
    {
        private readonly string topic = "email-message-topic";
        private readonly ITelegramSender _sender;
        private string _bootstrapServers;

        public KafkaConsumerEmailTelegramHandler(ITelegramSender sender, IConfiguration config)
        {
            _sender = sender;
            _bootstrapServers = config["Kafka:Broker"];
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = new ConsumerConfig
            {
                GroupId = "st_consumer_group",
                BootstrapServers = _bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Latest
            };
            using (var builder = new ConsumerBuilder<Ignore,
                string>(conf).Build())
            {
                builder.Subscribe(topic);
                var cancelToken = new CancellationTokenSource();
                try
                {
                    while (true)
                    {
                        var consumer = builder.Consume(cancelToken.Token);
                        var jObject = JObject.Parse(consumer.Message.Value);
                        var phone = (string)jObject["phone"];
                        var email = (string)jObject["email"];
                        _sender.SendMessage($"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}", phone);
                        Console.WriteLine($"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    builder.Close();
                }
            }
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}