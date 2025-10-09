using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQBus;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RabbitMQRpc
{
    public abstract class RabbitMQRpcBaseClientInitializer : RabbitMQBusBasePublisherInitializer, IRabbitMQRpcClientInitializer
    {
        public string? replyQueueName { get; protected set; } =null;
        public ConcurrentDictionary<string, TaskCompletionSource<JsonNode?>> callbackMap { get; } = new();
        public virtual ushort? Concurrency { get; protected set; } = 1;
        public override CreateChannelOptions? CreateChannelOptions => new CreateChannelOptions(false, false, consumerDispatchConcurrency: Concurrency);
        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            replyQueueName = (await Channel!.QueueDeclareAsync(cancellationToken: cancellationToken)).QueueName;
            var consumer = new AsyncEventingBasicConsumer(Channel!);
            consumer.ReceivedAsync += async (sender, args) => {
                var key = args.BasicProperties.CorrelationId;
                if (callbackMap.TryGetValue(key, out var task))
                {
                    var api = JsonSerializer.Deserialize<JsonNode>(args.Body.ToArray(),jsonTypeInfo: RabbitMQClientJsonContext.Default.JsonNode);
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
