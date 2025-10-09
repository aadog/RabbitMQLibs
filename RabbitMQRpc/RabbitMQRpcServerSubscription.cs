using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQBus;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RabbitMQRpc
{
    public class RabbitMQRpcServerSubscription(string queueName,Delegate value,ushort ? _concurrency) :RabbitMQBusBaseSubscription
    {
        public override string QueueName { get; } = queueName;
        public override CreateChannelOptions? CreateChannelOptions { get; protected set; } = new CreateChannelOptions(false,false,consumerDispatchConcurrency: _concurrency);
        
        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            await Channel!.QueueDeclareAsync(QueueName,true,false,true, cancellationToken:cancellationToken).ConfigureAwait(false);
        }
        public override async Task HandleMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            var response = new RpcResponse();
            var props = new BasicProperties
            {
                CorrelationId = args.BasicProperties.CorrelationId,
            };
            try
            {

                var funcType = value.Method.GetParameters();
                
                var argsArray = JsonSerializer.Deserialize<JsonNode[]>(args.Body.ToArray(), RabbitMQClientJsonContext.Default.JsonNodeArray)!;
                List<object> callArgs = argsArray.Select(object? (x) => x).ToList<object>();
                object? ret = null;
                if (funcType.Length > 0)
                {
                    if (funcType[^1].ParameterType==typeof(CancellationToken))
                    {
                        callArgs.Add(cancellationToken);
                        ret = value.DynamicInvoke(callArgs.ToArray());
                    }
                    else {
                        ret = value.DynamicInvoke(argsArray.ToArray<object?>());
                    }
                }
                else {
                    ret = value.DynamicInvoke(argsArray.ToArray());
                }
                if (ret is Task task)
                {
                    response.Data = await (dynamic)task;
                }
                else
                {
                    response.Data = ret;
                }
            }
            catch (Exception exception)
            {
                response.Error = exception.Message;
            }
            finally {
                try {
                    await Channel!.BasicPublishAsync("", args.BasicProperties.ReplyTo!, mandatory: false, basicProperties: props,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response,jsonTypeInfo:RabbitMQClientJsonContext.Default.RpcResponse)));
                }
                catch(Exception e){
                    Console.WriteLine($"err:{e.Message}");
                    //发生任何错误都不处理
                }
            }
        }
    }
}
