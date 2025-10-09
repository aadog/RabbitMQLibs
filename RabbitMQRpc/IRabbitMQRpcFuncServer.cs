using System.Collections.Concurrent;

namespace RabbitMQRpc
{
    public interface IRabbitMQRpcFuncServer
    {
        public string? Prefix { get;}
        public ConcurrentDictionary<string, Delegate> CallMaps { get; }
    }
}
