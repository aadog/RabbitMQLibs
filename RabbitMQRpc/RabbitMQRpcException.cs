using RabbitMQCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQRpc
{
    public class RabbitMQRpcException(string message):RabbitMQException(message)
    {
    }
}
