using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQBus;
using RabbitMQCommon;
using RabbitMQRpc;

namespace RabbitMQInitializers.HostedService
{
   
    public class RabbitMqInitializer(
    IEnumerable<IConnectionFactory> _connectionFactories,
    IServiceProvider _serviceProvider,
    IHostApplicationLifetime _hostApplicationLifetime
    ) : IHostedService
    {
        public async Task<bool> startComponent(IRabbitMqComponent component, IConnectionFactory connectionFactory, CancellationToken cancellationToken) {
            if (!component.IsStarted)
            {
                await component.StartAsync(connectionFactory, cancellationToken);
                component.IsStarted = true;
                return true;
            }
            return false;
        }
        public async Task StartWithConnectionFactoryAsync(IConnectionFactory connectionFactory, CancellationToken cancellationToken) {

            var clients = _serviceProvider.GetKeyedServices<IConnection>(connectionFactory.ClientProvidedName).ToArray();
            foreach (var item in clients)
            {
            }

            var publishers = _serviceProvider.GetKeyedServices<RabbitMQBusPublisher>(connectionFactory.ClientProvidedName).ToArray();
            foreach (var item in publishers)
            {
                await startComponent(item, connectionFactory, cancellationToken);
            }
            var consumers = _serviceProvider.GetKeyedServices<RabbitMQBusConsumer>(connectionFactory.ClientProvidedName).ToArray();
            foreach (var item in consumers)
            {
                await startComponent(item, connectionFactory, cancellationToken);
            }
            var rpcClients = _serviceProvider.GetKeyedServices<RabbitMQRpcClient>(connectionFactory.ClientProvidedName).ToArray();
            foreach (var item in rpcClients)
            {
                await startComponent(item, connectionFactory, cancellationToken);
            }
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var connectionFactory in _connectionFactories)
            {
                await StartWithConnectionFactoryAsync(connectionFactory, _hostApplicationLifetime.ApplicationStopped);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}