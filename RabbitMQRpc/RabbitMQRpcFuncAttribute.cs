namespace RabbitMQRpc
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited =false)]
    public class RabbitMQRpcFuncAttribute:Attribute
    {
        public bool AutoAck { get; set; } = true;
        public ushort Concurrency{ get; set; } = 1;
    }
}
