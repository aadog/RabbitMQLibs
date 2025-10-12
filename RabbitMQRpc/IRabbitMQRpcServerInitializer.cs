using RabbitMQBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RabbitMQRpc
{
    public interface IRabbitMQRpcServerInitializer: IRabbitMQBusConsumerInitializer
    {
        public JsonSerializerOptions? JsonSerializerOptions { get; set; }
        public IEnumerable<IRabbitMQRpcFuncServer> FuncServers { get; set; }
    }
}
