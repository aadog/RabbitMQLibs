using RabbitMQ.Client;
using RabbitMQCommon;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using RabbitMQBus;

namespace RabbitMQRpc
{
    public class RabbitMQRpcClient(IRabbitMQRpcClientInitializer initializer) : RabbitMQBusPublisher(initializer)
    {
        public virtual IRabbitMQRpcClientInitializer Initializer => initializer;
        public async Task<T?> CallAsync<T>(string funcName, object[] args,JsonTypeInfo? typeInfo=null, CancellationToken cancellationToken = default)
        {
            if (IsStarted == false)
            {
                throw new RabbitMQRpcException("rabbitMq not start");
            }
            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<JsonNode?>();
            await using var cancellationTokenRegistration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            initializer.RegisterRpcCall(correlationId, tcs);
      
            try
            {
                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(args, options: Initializer.JsonSerializerOptions));
                var props = new BasicProperties()
                {
                    CorrelationId = correlationId,
                    ReplyTo = Initializer.ReplyQueueName
                };
                var routingKey = $"{funcName}";
                if (Connection.ClientProvidedName != null)
                {
                    routingKey = $"{Connection.ClientProvidedName}.{funcName}";
                }
                await BasicPublishAsync(
                    "",
                    routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: messageBodyBytes,
                    cancellationToken: cancellationToken);
                if (typeInfo == null)
                {
                    return (await tcs.Task).Deserialize<T>(options: Initializer.JsonSerializerOptions);
                }
                return (T)(await tcs.Task).Deserialize(typeInfo)!;
            }
            catch (TaskCanceledException)
            {
                throw new RabbitMQRpcCallTimeoutException("timeout");
            }
            finally
            {
                Initializer.UnRegisterRpcCall(correlationId);
            }
        }

    }
}
