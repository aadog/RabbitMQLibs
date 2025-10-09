using RabbitMQRpc;
using System.Text.Json.Nodes;

namespace RabbitMQLibsTest
{
    public sealed class TestRpcFuncServer:RabbitMQRpcBaseFuncServer
    {
        public TestRpcFuncServer(string? Name=null)
        {
            RegisterMethod("test", zz1);
            this.Prefix= Name; 
        }
        [RabbitMQRpcFunc(Concurrency =10)]
        public async Task<string> zz1(JsonNode? b,CancellationToken cancellationToken) {
            return "zz";
        }
    }
}
