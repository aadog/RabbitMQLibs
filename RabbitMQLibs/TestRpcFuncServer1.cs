using RabbitMQRpc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace RabbitMQLibsTest
{
    public class TestRpcFuncServer1: RabbitMQRpcBaseFuncServer
    {
        public TestRpcFuncServer1(string? Name = null)
        {
            RegisterMethod("test", zz1);
            this.Prefix = Name;
        }
        [RabbitMQRpcFunc(Concurrency = 10)]
        public Task<string> zz1(JsonNode? b, CancellationToken cancellationToken)
        {
            return Task.FromResult("zz");
        }
    }
}
