namespace RabbitMQRpc
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited =false)]
    public class RabbitMQRpcFuncAttribute:Attribute
    {
        public ushort Concurrency = 1;
    }
}
