using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQBus;
using RabbitMQInitializers.HostedService;
using RabbitMQRpc;

namespace RabbitMQInitializers
{
    public static class RabbitMQServiceExtensions
    {
        public static IServiceCollection AddRabbitMQConnectionFactory(this IServiceCollection services,ConnectionFactory connectionFactory)
        {
            services.AddSingleton<IConnectionFactory>(connectionFactory);
            return services;
        }
        public static IServiceCollection AddRabbitMQConnectionFactoryWithUrlString(this IServiceCollection services,string urlString) {
            var p = new Uri(urlString);
            var connectFactory = new ConnectionFactory() {
                Uri=p,
            };
            AddRabbitMQConnectionFactory(services, connectFactory);
            return services;
        }
        public static IServiceCollection AddRabbitMQRawConnection(this IServiceCollection services, string? clientProvidedName = null)
        {
            //services.AddSingleton<RawRabbitMQConnection.RawRabbitMQConnection>(ops => new RawRabbitMQConnection.RawRabbitMQConnection() { ClientProvidedName = clientProvidedName });
            return services;
        }

        public static IServiceCollection AddRabbitMQBusConsumer<TConsumerInitializer>(this IServiceCollection services, string? clientProvidedName = null)
            where TConsumerInitializer : class, IRabbitMQBusConsumerInitializer, new()
        {
            services.AddKeyedSingleton<RabbitMQBusConsumer>(clientProvidedName, (sp, key) => new RabbitMQBusConsumer(new TConsumerInitializer()));
            return services;
        }
        public static IServiceCollection AddRabbitMQBusPublisher<TPublisherInitializer>(this IServiceCollection services, string? clientProvidedName = null)
            where TPublisherInitializer : class, IRabbitMQBusPublisherInitializer, new()
        {
            services.AddKeyedSingleton<RabbitMQBusPublisher>(clientProvidedName, (sp, key) =>new RabbitMQBusPublisher(new TPublisherInitializer()));
            return services;
        }
        public static IServiceCollection AddRabbitMQRpcClient<TRabbitMQRpcClientInitializer>(this IServiceCollection services, string? clientProvidedName = null)
            where TRabbitMQRpcClientInitializer : class, IRabbitMQRpcClientInitializer,new()
        {
            services.AddKeyedSingleton<RabbitMQRpcClient>(clientProvidedName, (sp, key) => new RabbitMQRpcClient(new TRabbitMQRpcClientInitializer()));
            return services;
        }
        public static IServiceCollection AddRabbitMQRpcServer<TRabbitMQRpcFuncServer>(this IServiceCollection services, IEnumerable<TRabbitMQRpcFuncServer> funcServers, string? clientProvidedName = null)
            where TRabbitMQRpcFuncServer:class,IRabbitMQRpcFuncServer
        {
            services.AddKeyedSingleton<RabbitMQBusConsumer>(clientProvidedName, (sp, key) => new RabbitMQBusConsumer(new RabbitMQRpcServerInitializer(funcServers)));
            return services;
        }
        public static IServiceCollection AddRabbitMQClient(this IServiceCollection services,string? clientProvidedName = null) {
            services.AddKeyedSingleton<IConnection>(clientProvidedName,  (sp,o) => {
                var connectionFactory = sp.GetServices<IConnectionFactory>().First(p=>p.ClientProvidedName== clientProvidedName);
                var connection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
                return connection;
            });
            return services;
        }

        /// <summary>
        /// 使用默认配置添加RabbitMQ RPC服务
        /// </summary>
        public static IServiceCollection AddRabbitMQInitializerService(this IServiceCollection services)
        {
            // 注册初始化托管服务
            services.AddHostedService<RabbitMqInitializer>();
            return services;
        }
    }
}
