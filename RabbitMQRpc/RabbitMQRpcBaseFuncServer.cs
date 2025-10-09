using System.Collections.Concurrent;

namespace RabbitMQRpc
{
    public abstract class RabbitMQRpcBaseFuncServer:IRabbitMQRpcFuncServer
    {

        public virtual string? Prefix { get; protected set; } = null;
        public ConcurrentDictionary<string, Delegate> CallMaps { get; }=new ConcurrentDictionary<string, Delegate>();
        public void RegisterMethod(string methodName, Delegate method)
        {
            CallMaps.AddOrUpdate(methodName, method, (k, v) => v);
        }
    }
}
