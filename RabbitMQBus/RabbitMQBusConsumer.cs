using RabbitMQ.Client;
using RabbitMQCommon;
namespace RabbitMQBus
{
    public class RabbitMQBusConsumer(IRabbitMQBusConsumerInitializer initializer) :IRabbitMqComponent
    {
        public async Task StartAsync(IConnectionFactory connectionFactory, CancellationToken cancellationToken = default)
        {
            if (connectionFactory.ClientProvidedName != null)
            {
                _connection = await connectionFactory.CreateConnectionAsync(connectionFactory.ClientProvidedName, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _connection = await connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            // 启动所有订阅的初始化（每个订阅自行处理细节）
            await initializer.InitializeAsync(Tag,_connection, cancellationToken).ConfigureAwait(false);
            IsStarted = true;
        }
        public string? ClientProvidedName { get; set; }
        public bool IsStarted { get; set; }
        public IConnection? _connection { get; set; } = null;
        public IConnection Connection=>_connection!;
        public string? Tag { get; set; } 
        protected bool _isDisposed;
        public virtual IRabbitMQBusConsumerInitializer Initializer => initializer;

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
            await initializer.DisposeAsync().ConfigureAwait(false);
            if (_connection != null) await _connection.DisposeAsync().ConfigureAwait(false);
            
        }
    }
}
