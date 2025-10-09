using RabbitMQBus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace RabbitMQRpc
{
    public interface IRabbitMQRpcClientInitializer:IRabbitMQBusPublisherInitializer
    {
        public string? replyQueueName { get; }
        public ConcurrentDictionary<string, TaskCompletionSource<JsonNode?>> callbackMap { get; }
        public ushort? Concurrency { get; }
    }
}
