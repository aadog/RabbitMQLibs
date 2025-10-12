using RabbitMQBus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RabbitMQRpc
{
    public interface IRabbitMQRpcClientInitializer:IRabbitMQBusPublisherInitializer
    {
        public string? ReplyQueueName { get; }
        public void RegisterRpcCall(string callId, TaskCompletionSource<JsonNode?> tcs);
        public void UnRegisterRpcCall(string callId);
        public ushort? Concurrency { get; }
        public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    }
}
