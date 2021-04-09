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
    public class KafkaConsumerPhoneSmsHandler : IHostedService
    {
        private readonly string topic = "phone-message-topic";
        private readonly ISmsSender _sender;
        private string _bootstrapServers;

        public KafkaConsumerPhoneSmsHandler(ISmsSender sender, IConfiguration config)
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
                        var phone = (string) jObject["phone"];
                        var email = (string) jObject["email"];
                        _sender.SendSms(new SmsMessage
                        {
                            From = "+19149964683",
                            Message = $"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}",
                            To = "+380638680809"
                        });
                        Console.WriteLine($"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}");
                    }
                }
                catch (Exception)
                {
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