using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace RabbitMQRpc
{
    [JsonSerializable(typeof(JsonNode[]))]
    [JsonSerializable(typeof(RpcResponse[]))]
    [JsonSerializable(typeof(JsonArray[]))]
    [JsonSerializable(typeof(object[]))]
    [JsonSerializable(typeof(JsonObject[]))]
    [JsonSerializable(typeof(JsonElement[]))]
    public partial class RabbitMQClientJsonContext : JsonSerializerContext { }
}
