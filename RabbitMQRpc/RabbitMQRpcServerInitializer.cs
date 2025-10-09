using RabbitMQ.Client;
using RabbitMQBus;
using System.Reflection;

namespace RabbitMQRpc
{
    public class RabbitMQRpcServerInitializer(IEnumerable<IRabbitMQRpcFuncServer> _funcServers) :RabbitMQBusBaseConsumerInitializer
    {
        public override Task InitializeAsync(IConnection connection, CancellationToken cancellationToken)
        {
            foreach (var server in _funcServers) {
                foreach (var fn in server.CallMaps)
                {
                    var rpcFuncAttribute = fn.Value.Method.GetCustomAttribute<RabbitMQRpcFuncAttribute>();
                    var queueName = $"{server.Prefix}.{fn.Key}";
                    if (server.Prefix == null)
                    {
                        queueName = $"{fn.Key}";
                    }
                    else {
                        queueName = $"{server.Prefix}.{fn.Key}";
                    }
                    if (connection.ClientProvidedName != null)
                    {
                        if (server.Prefix == null)
                        {
                            queueName = $"{connection.ClientProvidedName}.{fn.Key}";
                        }
                        else {
                            queueName = $"{connection.ClientProvidedName}.{server.Prefix}.{fn.Key}";
                        }
                    }
                    AddSubscription(new RabbitMQRpcServerSubscription(queueName, fn.Value, rpcFuncAttribute?.Concurrency));
                }
            }
            return base.InitializeAsync(connection, cancellationToken);
        }
    }
}
