using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQBus;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RabbitMQRpc
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class RabbitMQRpcServerSubscription:RabbitMQBusBaseSubscription
    {
        public Delegate Delegate { get; set; } = null!;
        public ushort? Concurrency { get; set; } = null;
        public JsonSerializerOptions? jsonSerializerOptions { get; set; }
        public override string QueueName { get; set; } = null!;
        public override CreateChannelOptions? CreateChannelOptions { get; protected set; }

        public RabbitMQRpcServerSubscription(IServiceProvider serviceProvider) :base(serviceProvider)
        {   
            CreateChannelOptions= new CreateChannelOptions(false, false, consumerDispatchConcurrency: Concurrency); 
        }

        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            await Channel!.QueueDeclareAsync(QueueName,true,false,true, cancellationToken:cancellationToken).ConfigureAwait(false);
        }

        // 这是一个 AOT 安全的方法，因为 C# 编译器知道如何处理泛型 Task<TResult> 的 await。
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Helper method to safely await and return Task<T> result.")]
        private static async Task<object?> GetTaskResultAotSafe<TResult>(Task<TResult> task)
        {
            // C# 编译器在编译时会为每个具体类型 TResult 生成优化的代码，
            // 因此它能安全地访问结果，绕过了反射问题。
            return await task.ConfigureAwait(false);
        }

        public override async Task HandleMessageAsync(BasicDeliverEventArgs args,IChannel channel, CancellationToken cancellationToken)
        {
            var response = new RpcResponse();
            var props = new BasicProperties
            {
                CorrelationId = args.BasicProperties.CorrelationId,
            };
            try
            {

                var funcType = Delegate.Method.GetParameters();
                var funcReturnType = Delegate.Method.ReturnType;
                var argsArray = JsonSerializer.Deserialize(args.Body.ToArray(),jsonTypeInfo:RabbitMQClientJsonContext.Default.JsonNodeArray)!;
                List<object> callArgs = argsArray.Select((x) => x).ToList<object>();
                object? ret = null;
                if (funcType.Length > 0)
                {
                    using var scope = ServiceProvider.CreateScope();
                    var scopeProvider= scope.ServiceProvider;
                    List<object> diCallArgs=new List<object>();

                    for (int i = 0; i < funcType.Length; i++){
                        var argType = funcType[i].ParameterType;
                        if (!typeof(JsonNode).IsAssignableFrom(argType)) {
                            if (argType == typeof(CancellationToken))
                            {
                                diCallArgs.Add(cancellationToken);
                                continue;
                            }
                            var attr= funcType[i].GetCustomAttribute<FromKeyedServicesAttribute>();
                            if (attr != null)
                            {
                                var service = scopeProvider.GetRequiredKeyedService(argType, attr.Key);
                                diCallArgs.Add(service);
                                continue;
                            }
                            else {
                                var service = scopeProvider.GetRequiredService(argType);
                                diCallArgs.Add(service);
                                continue;
                            }
                        }

                        diCallArgs.Add(DBNull.Value);
                    }

                    var diNullCallArgsCount = diCallArgs.Count(d => d == DBNull.Value);
                    if (diNullCallArgsCount != callArgs.Count) {
                        throw new Exception($"input arg count {callArgs.Count} func arg count {diNullCallArgsCount}");
                    }
                    for (int i = 0; i < callArgs.Count; i++) {
                        for (int j = 0; j < diCallArgs.Count; j++) {
                            if (diCallArgs[j] == DBNull.Value)
                            {
                                diCallArgs[j] = callArgs[i];
                                break;
                            }
                        }
                    }
                    ret = Delegate.DynamicInvoke(diCallArgs.ToArray());
                }
                else {
                    ret = Delegate.DynamicInvoke();
                }
                if (ret is Task task)
                {

                    await task.ConfigureAwait(false);
                    if (funcReturnType.IsGenericType && funcReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        Task<object> typedTask = (Task<object>)task;
                        response.Data = await GetTaskResultAotSafe(typedTask);
                    }
                    else {
                        response.Data = null;
                    }
                    
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
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response, jsonSerializerOptions)), cancellationToken: cancellationToken);
                }
                catch(Exception e){
                    Console.WriteLine($"err:{e.Message}");
                    //发生任何错误都不处理
                }
            }
        }
    }
}
