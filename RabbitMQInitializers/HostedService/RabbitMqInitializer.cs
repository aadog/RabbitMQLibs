using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQCommon;

namespace RabbitMQInitializers.HostedService
{
   
    public class RabbitMqInitializer(
    IServiceProvider serviceProvider
    ) : IHostedService
    {
        public async Task<bool> startComponent(IRabbitMqComponent component, IConnectionFactory connectionFactory, CancellationToken cancellationToken) {
            if (!component.IsStarted)
            {
                await component.StartAsync(connectionFactory, cancellationToken);
                return true;
            }
            return false;
        }
        public async Task StartWithConnectionFactoryAsync(IConnectionFactory connectionFactory, CancellationToken cancellationToken) {
            var components = serviceProvider.GetServices<IRabbitMqComponent>().ToArray();
            foreach (var item in components)
            {
                if (item.ClientProvidedName == connectionFactory.ClientProvidedName) {
                    await startComponent(item, connectionFactory, cancellationToken);
                }
            }
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var connectionFactories = serviceProvider.GetServices<IConnectionFactory>();
            var hostApplicationLifetime=serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            foreach (var connectionFactory in connectionFactories)
            {
                await StartWithConnectionFactoryAsync(connectionFactory, hostApplicationLifetime.ApplicationStopped);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}