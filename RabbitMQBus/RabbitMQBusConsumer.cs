using RabbitMQ.Client;
using RabbitMQCommon;

namespace RabbitMQBus
{
    public class RabbitMQBusConsumer(IRabbitMQBusConsumerInitializer _initializer) :IRabbitMqComponent,IDisposable,IAsyncDisposable
    {
        public async Task StartAsync(IConnectionFactory connectionFactory, CancellationToken cancellationToken = default)
        {
            if (connectionFactory.ClientProvidedName != null)
            {
                connection = await connectionFactory.CreateConnectionAsync(connectionFactory.ClientProvidedName, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                connection = await connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            // 启动所有订阅的初始化（每个订阅自行处理细节）
            await _initializer.InitializeAsync(connection, cancellationToken).ConfigureAwait(false);
        }

        public bool IsStarted { get; set; }
        public IConnection? connection { get; private set; }
        protected bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed) return;
            _initializer.Dispose();
            connection?.Dispose();
            _isDisposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            await _initializer.DisposeAsync().ConfigureAwait(false);
            if (connection != null) await connection.DisposeAsync().ConfigureAwait(false);
            _isDisposed = true;
        }
    }
}
