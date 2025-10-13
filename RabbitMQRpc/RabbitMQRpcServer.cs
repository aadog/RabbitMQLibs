using RabbitMQBus;
using RabbitMQCommon;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMQRpc
{
    public class RabbitMQRpcServer(IRabbitMQRpcServerInitializer initializer):RabbitMQBusConsumer(initializer)
    {
        public virtual IRabbitMQRpcServerInitializer Initializer => initializer;

    }
}
