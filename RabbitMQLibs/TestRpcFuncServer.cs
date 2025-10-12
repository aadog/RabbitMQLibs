using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQBus;
using RabbitMQCommon;
using RabbitMQRpc;
using System.Text.Json.Nodes;

namespace RabbitMQLibsTest
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public sealed class TestRpcFuncServer:RabbitMQRpcBaseFuncServer
    {
        public TestRpcFuncServer(string? Name=null)
        {
            RegisterMethod("test", zz1);
            this.Prefix= Name; 
        }

        
        [RabbitMQRpcFunc(Concurrency =10)]
        public Task<object> zz1(JsonValue b,CancellationToken cancellationToken) {
            Console.WriteLine("bb");
            return Task.FromResult((object)"zz");
        }
    }
}
