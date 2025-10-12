using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQBus;
using RabbitMQInitializers;
using RabbitMQRpc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RabbitMQLibsTest
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddRabbitMQConnectionFactory(new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = true,
                HostName = "127.0.0.1",
                Port = 5672,
                UserName = "root",
                Password = "Zz1231231aaa!@#",
            });
            builder.Services.AddRabbitMQBusPublisher<PublisherInit>(null);
            builder.Services.AddRabbitMQBusConsumer<ConsumerInit>(null);

            builder.Services.AddRabbitMQRpcClient<RabbitMQRpcClientInitializer>(null);
            builder.Services.AddRabbitMQRpcServer<RabbitMQRpcServerInitializer>(null, [new TestRpcFuncServer(), new TestRpcFuncServer1()]);
            builder.Services.AddRabbitMQClient();
            builder.Services.AddRabbitMQInitializerService();
            var app = builder.Build();
            var appLifetime = app.Services.GetService<IHostApplicationLifetime>()!;
            appLifetime.ApplicationStarted.Register(async () =>
            {
                //var consumer = app.Services.GetRequiredKeyedService<RabbitMQBusConsumer<ConsumerInit>>(null);
                var publisher = app.Services.GetRequiredKeyedService<RabbitMQBusPublisher<PublisherInit>>(null);

                var props = new BasicProperties();
                var body = Encoding.UTF8.GetBytes("hello 发送消息了");
                for (int i = 0; i < 10000000000; i++) {
                    await publisher.BasicPublishAsync("", "bb", false,props,body);
                }




                ////var publisher = app.Services.GetRequiredKeyedService<RabbitMQRpcServer<RabbitMQRpcServerInitializer>>(null);


                //var publisher = app.Services.GetRequiredKeyedService<RabbitMQRpcClient<RabbitMQRpcClientInitializer>>(null);

                //await Parallel.ForAsync(0, 100000, async (i, c) =>
                //{
                //    for (int j = 0; j < 100000; j++) {
                //        var z = await publisher.CallAsync<string>("test", ["1"]);
                //        Console.WriteLine(z);
                //    }

                //});
            });
           
            await app.RunAsync();
        }
    }


    [JsonSerializable(typeof(string))]
    public partial class AppJsonContext : JsonSerializerContext
    {

    }
}