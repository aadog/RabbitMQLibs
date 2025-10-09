using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQBus;
using RabbitMQCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;

namespace RabbitMQRpc
{
    public class RabbitMQRpcClientInitializer:RabbitMQBusBasePublisherInitializer
    {
        public string? replyQueueName = null;
        public readonly ConcurrentDictionary<string, TaskCompletionSource<JsonNode?>> callbackMap = new();
        public ushort? Concurrency { get; set; } = 1;
        public override CreateChannelOptions? CreateChannelOptions => new CreateChannelOptions(false, false, consumerDispatchConcurrency: Concurrency);
        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            replyQueueName = (await Channel!.QueueDeclareAsync(cancellationToken: cancellationToken)).QueueName;
            var consumer = new AsyncEventingBasicConsumer(Channel!);
            consumer.ReceivedAsync += async (sender, args) => {
                var key = args.BasicProperties.CorrelationId;
                if (callbackMap.TryGetValue(key, out var task))
                {
                    var api = JsonSerializer.Deserialize<JsonNode>(args.Body.ToArray());
                    if (api["Error"] != null)
                    {
                        task.SetException(new Exception(api["Error"]!.GetValue<string>()));
                    }
                    else
                    {
                        task.SetResult(api["Data"]);
                    }
                }
            };
            await Channel!.BasicConsumeAsync(replyQueueName, true, consumer, cancellationToken);
        }

    }
}
