using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQBus;
using RabbitMQCommon;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace RabbitMQRpc
{
    public class RabbitMQRpcClient(IRabbitMQRpcClientInitializer _initializer) : RabbitMQBusPublisher(_initializer)
    {
        public async Task<T?> CallAsync<T>(string funcName, object[] args,JsonTypeInfo? typeInfo=null, CancellationToken cancellationToken = default)
        {
            if (IsStarted == false)
            {
                throw new RabbitMQRpcException("rabbitMq not start");
            }
            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<JsonNode?>();
            await using var cancellationTokenRegistration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            _initializer.callbackMap.TryAdd(correlationId, tcs);
            try
            {
                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(args, RabbitMQClientJsonContext.Default.ObjectArray));
                var props = new BasicProperties()
                {
                    CorrelationId = correlationId,
                    ReplyTo = _initializer.replyQueueName
                };
                var routingKey = $"{funcName}";
                if (connection!.ClientProvidedName != null) {
                    routingKey = $"{connection.ClientProvidedName}.{funcName}";
                }
                await BasicPublishAsync(
                    "",
                    routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: messageBodyBytes,
                    cancellationToken: cancellationToken);
                if (typeInfo == null){
                    return (await tcs.Task).Deserialize<T>();
                }
                return (T)(await tcs.Task).Deserialize(typeInfo)!;
            }
            catch (TaskCanceledException)
            {
                throw new RabbitMQRpcCallTimeoutException("timeout");
            }
            catch (Exception e)
            {
                _initializer.callbackMap.Remove(correlationId, out _);
                throw;
            }
        }

    }

    [JsonSerializable(typeof(JsonNode[]))]
    [JsonSerializable(typeof(RpcResponse[]))]
    [JsonSerializable(typeof(JsonArray[]))]
    [JsonSerializable(typeof(object[]))]
    public partial class RabbitMQClientJsonContext : JsonSerializerContext{ }
}
