using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQBus;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;

namespace RabbitMQRpc
{
    public class RabbitMQRpcServerInitializer(IServiceProvider serviceProvider):RabbitMQBusBaseConsumerInitializer(serviceProvider),IRabbitMQRpcServerInitializer
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public IEnumerable<IRabbitMQRpcFuncServer> FuncServers { get; set; } = [];
        public JsonSerializerOptions? JsonSerializerOptions { get; set; }

        [RequiresUnreferencedCodeAttribute("1")]
        public override Task InitializeAsync(string? Tag,IConnection connection, CancellationToken cancellationToken)
        {
            var filterDict=new Dictionary<string,bool>();
            foreach (var server in FuncServers)
            {
                foreach (var fn in server.CallMaps)
                {
                    var rpcFuncAttribute = fn.Value.Method.GetCustomAttribute<RabbitMQRpcFuncAttribute>();
                    var queueName = $"{server.Prefix}.{fn.Key}";
                    if (server.Prefix == null)
                    {
                        queueName = $"{fn.Key}";
                    }
                    else
                    {
                        queueName = $"{server.Prefix}.{fn.Key}";
                    }
                    if (connection.ClientProvidedName != null)
                    {
                        if (server.Prefix == null)
                        {
                            queueName = $"{connection.ClientProvidedName}.{fn.Key}";
                        }
                        else
                        {
                            queueName = $"{connection.ClientProvidedName}.{server.Prefix}.{fn.Key}";
                        }
                    }
                    if (filterDict.TryGetValue(queueName, out _) == true) {
                        throw new Exception($"{queueName} is exist");
                    }
                    filterDict.Add(queueName, true);
                    var serverSubscription =ActivatorUtilities.CreateInstance<RabbitMQRpcServerSubscription>(_serviceProvider);
                    serverSubscription.AutoAck = rpcFuncAttribute?.AutoAck??true;
                    serverSubscription.QueueName = queueName;
                    serverSubscription.Delegate = fn.Value;
                    serverSubscription.Concurrency = rpcFuncAttribute?.Concurrency;
                    serverSubscription.jsonSerializerOptions = JsonSerializerOptions;
                    AddSubscription(serverSubscription);
                }
            }
            return base.InitializeAsync(Tag,connection, cancellationToken);
        }
    }
}
