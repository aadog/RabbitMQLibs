using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using RabbitMQBus;
using RabbitMQCommon;
using RabbitMQInitializers.HostedService;
using RabbitMQRpc;
using System.Text.Json;

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

        public static IServiceCollection AddRabbitMQBusConsumer<TRabbitMQBusConsumerInitializer>(this IServiceCollection services, string? tag, string? clientProvidedName = null)
            where TRabbitMQBusConsumerInitializer : class, IRabbitMQBusConsumerInitializer
        {
            services.TryAddTransient<TRabbitMQBusConsumerInitializer>();
            services.AddKeyedSingleton<RabbitMQBusConsumer<TRabbitMQBusConsumerInitializer>>(clientProvidedName, (sp, key) =>
            {
                var initializer = (TRabbitMQBusConsumerInitializer)ActivatorUtilities.CreateInstance(sp, typeof(TRabbitMQBusConsumerInitializer), []);


                var component = (RabbitMQBusConsumer<TRabbitMQBusConsumerInitializer>)ActivatorUtilities.CreateInstance(sp, typeof(RabbitMQBusConsumer<TRabbitMQBusConsumerInitializer>), [initializer]);
                component.Tag = tag;
                return component;
            });
            services.AddKeyedSingleton<IRabbitMqComponent>(clientProvidedName, (sp, k) => sp.GetKeyedServices<RabbitMQBusConsumer<TRabbitMQBusConsumerInitializer>>(clientProvidedName).First(e => e.Tag == tag));
            return services;
        }
        public static IServiceCollection AddRabbitMQBusPublisher<TRabbitMQBusPublisherInitializer>(this IServiceCollection services,string? tag, string? clientProvidedName = null)
            where TRabbitMQBusPublisherInitializer : class, IRabbitMQBusPublisherInitializer, new()
        {
            services.TryAddTransient<TRabbitMQBusPublisherInitializer>();
            services.AddKeyedSingleton<RabbitMQBusPublisher<TRabbitMQBusPublisherInitializer>>(clientProvidedName, (sp, key) => {
                var initializer = (TRabbitMQBusPublisherInitializer)ActivatorUtilities.CreateInstance(sp, typeof(TRabbitMQBusPublisherInitializer), []);


                var component = (RabbitMQBusPublisher<TRabbitMQBusPublisherInitializer>)ActivatorUtilities.CreateInstance(sp, typeof(RabbitMQBusPublisher<TRabbitMQBusPublisherInitializer>), [initializer]);
                component.Tag = tag;
                return component;
            });
            services.AddKeyedSingleton<IRabbitMqComponent>(clientProvidedName, (sp, k) => sp.GetKeyedServices<RabbitMQBusPublisher<TRabbitMQBusPublisherInitializer>>(clientProvidedName).First(e => e.Tag == tag));
            return services;
        }
        public static IServiceCollection AddRabbitMQRpcClient<TRabbitMQRpcClientInitializer>(this IServiceCollection services,string? tag, JsonSerializerOptions? jsonSerializerOptions = null, string? clientProvidedName = null)
            where TRabbitMQRpcClientInitializer:class, IRabbitMQRpcClientInitializer
        {
            services.TryAddTransient<RabbitMQRpcClientInitializer>();
            services.AddKeyedSingleton<RabbitMQRpcClient<TRabbitMQRpcClientInitializer>>(clientProvidedName, (sp, key) => {
                var initializer = (TRabbitMQRpcClientInitializer)ActivatorUtilities.CreateInstance(sp, typeof(TRabbitMQRpcClientInitializer), []);
                initializer.JsonSerializerOptions = jsonSerializerOptions;
                if (jsonSerializerOptions == null)
                {
                    initializer.JsonSerializerOptions = new JsonSerializerOptions()
                    {
                        TypeInfoResolver = RabbitMQClientJsonContext.Default
                    };
                }

                var component = (RabbitMQRpcClient<TRabbitMQRpcClientInitializer>)ActivatorUtilities.CreateInstance(sp, typeof(RabbitMQRpcClient<TRabbitMQRpcClientInitializer>), [initializer]);
                component.Tag = tag;
                return component;
            });
            services.AddKeyedSingleton<IRabbitMqComponent>(clientProvidedName, (sp, k) => sp.GetKeyedServices<RabbitMQRpcClient<TRabbitMQRpcClientInitializer>>(clientProvidedName).First(e => e.Tag == tag));
            return services;
        }
        public static IServiceCollection AddRabbitMQRpcServer<TRabbitMQRpcServerInitializer>(this IServiceCollection services,string? tag,IRabbitMQRpcFuncServer[] funcServers,JsonSerializerOptions? jsonSerializerOptions=null, string? clientProvidedName = null)
            where TRabbitMQRpcServerInitializer:class,IRabbitMQRpcServerInitializer
        {
            services.TryAddTransient<TRabbitMQRpcServerInitializer>();
            services.AddKeyedSingleton<RabbitMQRpcServer<TRabbitMQRpcServerInitializer>>(clientProvidedName, (sp, key) => {
                var initializer = (TRabbitMQRpcServerInitializer)ActivatorUtilities.CreateInstance(sp, typeof(TRabbitMQRpcServerInitializer), [funcServers]);
                initializer.JsonSerializerOptions = jsonSerializerOptions;
                if (jsonSerializerOptions == null)
                {
                    initializer.JsonSerializerOptions = new JsonSerializerOptions()
                    {
                        TypeInfoResolver = RabbitMQClientJsonContext.Default
                    };
                }
                var component = (RabbitMQRpcServer<TRabbitMQRpcServerInitializer>)ActivatorUtilities.CreateInstance(sp, typeof(RabbitMQRpcServer<TRabbitMQRpcServerInitializer>), [initializer]);
                component.Tag = tag;
                return component;
            });
            services.AddKeyedSingleton<IRabbitMqComponent>(clientProvidedName, (sp, k) => sp.GetKeyedServices<RabbitMQRpcServer<TRabbitMQRpcServerInitializer>>(clientProvidedName).First(e => e.Tag == tag));
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
