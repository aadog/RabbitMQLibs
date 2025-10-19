using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace RabbitMQBus
{
    public static class IChannelExtensions
    {
        extension(IChannel channel) {
            public async ValueTask BasicPublishObjectAsync<T, TProperties>(string exchange, string routingKey,
                bool mandatory, T body, TProperties basicProperties,
                JsonTypeInfo? jsonTypeInfo = null,
                CancellationToken cancellationToken = default)
                where TProperties : IReadOnlyBasicProperties, IAmqpHeader
            {
                byte[] b;
                b = jsonTypeInfo == null ? JsonSerializer.SerializeToUtf8Bytes(body) : JsonSerializer.SerializeToUtf8Bytes(body, jsonTypeInfo);
                await channel.BasicPublishAsync(exchange, routingKey, mandatory, basicProperties, b, cancellationToken); ;
            }
        }
    }
}
