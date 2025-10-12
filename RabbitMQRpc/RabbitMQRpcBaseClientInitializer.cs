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
        private string? _replyQueueName = null;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonNode?>> _callbackMap = new();
        public string? ReplyQueueName => _replyQueueName;
        public void RegisterRpcCall(string callId, TaskCompletionSource<JsonNode?> tcs)
        {
            _callbackMap.TryAdd(callId, tcs);
        }

        public void UnRegisterRpcCall(string callId)
        {
            _callbackMap.TryRemove(callId, out _);
        }

        public ushort? Concurrency { get; set; } = 1;
        public JsonSerializerOptions? JsonSerializerOptions { get; set; }
        public override CreateChannelOptions? CreateChannelOptions => new CreateChannelOptions(false, false, consumerDispatchConcurrency: Concurrency);
        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            _replyQueueName = (await Channel!.QueueDeclareAsync(cancellationToken: cancellationToken)).QueueName;
            var consumer = new AsyncEventingBasicConsumer(Channel!);
            consumer.ReceivedAsync += async (sender, args) => {
                var key = args.BasicProperties.CorrelationId;
                if (_callbackMap.TryGetValue(key, out var task))
                {
                    var api = JsonSerializer.Deserialize<JsonNode>(args.Body.ToArray(),options:JsonSerializerOptions);
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
            await Channel!.BasicConsumeAsync(_replyQueueName, true, consumer, cancellationToken);
        }
    }
}
