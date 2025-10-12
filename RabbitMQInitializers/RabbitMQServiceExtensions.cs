using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQBus;
using RabbitMQCommon;
using RabbitMQInitializers.HostedService;
using RabbitMQRpc;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

        public static IServiceCollection AddRabbitMQBusConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRabbitMQBusConsumerInitializer>(this IServiceCollection services, string? tag, string? clientProvidedName = null)
            where TRabbitMQBusConsumerInitializer : class, IRabbitMQBusConsumerInitializer
        {
            services.TryAddTransient<TRabbitMQBusConsumerInitializer>();
            services.AddKeyedSingleton(tag, (sp, key) =>
            {
                var initializer = sp.GetRequiredService<TRabbitMQBusConsumerInitializer>();
                var component = new RabbitMQBusConsumer<TRabbitMQBusConsumerInitializer>(initializer);
                component.Tag = tag;
                component.ClientProvidedName = clientProvidedName;
                return component;
            });
            services.AddSingleton<IRabbitMqComponent>((sp) => sp.GetRequiredKeyedService<RabbitMQBusConsumer<TRabbitMQBusConsumerInitializer>>(tag));
            return services;
        }
        public static IServiceCollection AddRabbitMQBusPublisher<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRabbitMQBusPublisherInitializer>(this IServiceCollection services,string? tag=null, string? clientProvidedName = null)
            where TRabbitMQBusPublisherInitializer : class, IRabbitMQBusPublisherInitializer
        {
            services.TryAddTransient<TRabbitMQBusPublisherInitializer>();
            services.AddKeyedSingleton(tag,(sp,key) => {
                var initializer = sp.GetRequiredService<TRabbitMQBusPublisherInitializer>();
                var component = new RabbitMQBusPublisher<TRabbitMQBusPublisherInitializer>(initializer);
                component.Tag = tag;
                component.ClientProvidedName = clientProvidedName;
                return component;
            });
            services.AddSingleton<IRabbitMqComponent>((sp) => sp.GetRequiredKeyedService<RabbitMQBusPublisher<TRabbitMQBusPublisherInitializer>>(tag));
            return services;
        }

        public static IServiceCollection AddRabbitMQRpcClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRabbitMQRpcClientInitializer>(this IServiceCollection services,string? tag, JsonSerializerOptions? jsonSerializerOptions = null, string? clientProvidedName = null)
            where TRabbitMQRpcClientInitializer:class, IRabbitMQRpcClientInitializer
        {
            services.TryAddTransient<RabbitMQRpcClientInitializer>();
            services.AddKeyedSingleton(tag, (sp, key) => {
                var initializer = sp.GetRequiredService<TRabbitMQRpcClientInitializer>();
                initializer.JsonSerializerOptions = jsonSerializerOptions;
                if (jsonSerializerOptions == null)
                {
                    initializer.JsonSerializerOptions = new JsonSerializerOptions()
                    {
                        TypeInfoResolver = RabbitMQClientJsonContext.Default
                    };
                }

                var component = new RabbitMQRpcClient<TRabbitMQRpcClientInitializer>(initializer);
                component.Tag = tag;
                component.ClientProvidedName = clientProvidedName;
                return component;
            });
           
            services.AddSingleton<IRabbitMqComponent>((sp) => sp.GetRequiredKeyedService<RabbitMQRpcClient<TRabbitMQRpcClientInitializer>>(tag));
            return services;
        }
   
        public static IServiceCollection AddRabbitMQRpcServer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRabbitMQRpcServerInitializer>(this IServiceCollection services,string? tag,IRabbitMQRpcFuncServer[] funcServers,JsonSerializerOptions? jsonSerializerOptions=null, string? clientProvidedName = null)
            where TRabbitMQRpcServerInitializer:class,IRabbitMQRpcServerInitializer
        {
            services.TryAddTransient<TRabbitMQRpcServerInitializer>();
            services.AddKeyedSingleton(clientProvidedName, (sp, key) => {
                var initializer = sp.GetRequiredService<TRabbitMQRpcServerInitializer>();
                initializer.FuncServers = funcServers;
                initializer.JsonSerializerOptions = jsonSerializerOptions;
                if (jsonSerializerOptions == null)
                {
                    initializer.JsonSerializerOptions = new JsonSerializerOptions()
                    {
                        TypeInfoResolver = RabbitMQClientJsonContext.Default
                    };
                }
                var component = new RabbitMQRpcServer<TRabbitMQRpcServerInitializer>(initializer);
                component.Tag = tag;
                component.ClientProvidedName = clientProvidedName;
                return component;
            });
            services.AddSingleton<IRabbitMqComponent>((sp) => sp.GetRequiredKeyedService<RabbitMQRpcServer<TRabbitMQRpcServerInitializer>>(tag));
            return services;
        }
        public static IServiceCollection AddRabbitMQClient(this IServiceCollection services,string? tag,string? clientProvidedName = null) {
            services.AddKeyedSingleton(tag,  (sp,o) => {
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
