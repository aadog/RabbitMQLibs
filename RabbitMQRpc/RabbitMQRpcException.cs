using RabbitMQCommon;

namespace RabbitMQRpc
{
    public class RabbitMQRpcException(string message):RabbitMQException(message)
    {
    }
}
