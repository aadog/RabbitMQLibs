using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQBus;
using RabbitMQInitializers;
using RabbitMQRpc;
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
            //builder.Services.AddRabbitMQBusPublisher<PublisherInit>();
            //builder.Services.AddRabbitMQBusConsumer<ConsumerInit>();

            builder.Services.AddRabbitMQRpcClient<TestRpcClientInitializer>();
            builder.Services.AddRabbitMQRpcServer([new TestRpcFuncServer()]);
            builder.Services.AddRabbitMQClient();
            builder.Services.AddRabbitMQInitializerService();
            var app = builder.Build();
            var appLifetime = app.Services.GetService<IHostApplicationLifetime>()!;
            appLifetime.ApplicationStarted.Register(async () =>
            {

                var publisher = app.Services.GetRequiredKeyedService<RabbitMQRpcClient>(null);

                await Parallel.ForAsync(0, 10, async (i, c) =>
                {
                    var z = await publisher.CallAsync<string>("test", ["1"], AppJsonContext.Default.String);
                    Console.WriteLine(z);

                });
            });
           
            await app.RunAsync();
        }
    }


    [JsonSerializable(typeof(string))]
    public partial class AppJsonContext : JsonSerializerContext
    {

    }
}