using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQRpc
{
    public record RpcResponse
    {
        public string? Error { get; set; }
        public object? Data { get; set; }
    }
}
