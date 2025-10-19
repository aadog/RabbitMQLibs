using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQCommon;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace RabbitMQBus
{
    public class RabbitMQBusPublisher(IRabbitMQBusPublisherInitializer initializer) : IRabbitMqComponent
    {
        public string? ClientProvidedName { get; set; }
        public bool IsStarted { get; set; }
        public IConnection Connection => _connection!;
        public string? Tag { get; set; }
        private IConnection? _connection { get; set; } = null;
        public virtual IRabbitMQBusPublisherInitializer Initializer => initializer;

        protected bool _isDisposed;
        public async Task StartAsync(IConnectionFactory connectionFactory, CancellationToken cancellationToken = default)
        {

            if (connectionFactory.ClientProvidedName != null)
            {
                _connection = await connectionFactory.CreateConnectionAsync(connectionFactory.ClientProvidedName, cancellationToken).ConfigureAwait(false);
            }
            else {
                _connection = await connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            await initializer.InitializeAsync(Tag,_connection, cancellationToken).ConfigureAwait(false);
            IsStarted = true;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            initializer.Dispose();
            _connection?.Dispose();
            
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            await initializer.DisposeAsync();
            if (_connection != null) await _connection.DisposeAsync().ConfigureAwait(false);
            
        }
    }
}
