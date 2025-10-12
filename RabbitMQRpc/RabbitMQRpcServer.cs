using RabbitMQBus;
using RabbitMQCommon;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMQRpc
{
    public class RabbitMQRpcServer<TRabbitMQRpcServerInitializer>(TRabbitMQRpcServerInitializer initializer):RabbitMQBusConsumer<TRabbitMQRpcServerInitializer>(initializer)
        where TRabbitMQRpcServerInitializer:class,IRabbitMQRpcServerInitializer
    {
   
       
    }
}
