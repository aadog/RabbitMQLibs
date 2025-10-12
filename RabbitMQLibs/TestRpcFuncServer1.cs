using RabbitMQRpc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQLibsTest
{
    public class TestRpcFuncServer1: RabbitMQRpcBaseFuncServer
    {
        public string? Prefix { get; }
        public ConcurrentDictionary<string, Delegate> CallMaps { get; }
    }
}
