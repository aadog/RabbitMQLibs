using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQCommon;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace RabbitMQBus
{
    public class RabbitMQBusPublisher(IRabbitMQBusPublisherInitializer _initializer) :IDisposable,IAsyncDisposable,IRabbitMqComponent
    {
        public bool IsStarted { get; set; }
        public IConnection? connection;
  
        protected bool _isDisposed;
        public async Task StartAsync(IConnectionFactory connectionFactory, CancellationToken cancellationToken = default)
        {
            if (connectionFactory.ClientProvidedName != null)
            {
                connection = await connectionFactory.CreateConnectionAsync(connectionFactory.ClientProvidedName, cancellationToken).ConfigureAwait(false);
            }
            else {
                connection = await connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            await _initializer.InitializeAsync(connection,cancellationToken).ConfigureAwait(false);
        }

        public ValueTask BasicPublishObjectAsync<T>(string exchange, string routingKey,
            bool mandatory, T body, BasicProperties? basicProperties=null,
            JsonTypeInfo? jsonTypeInfo=null,
            CancellationToken cancellationToken = default)
        {
            byte[] b;
            b = jsonTypeInfo == null ? JsonSerializer.SerializeToUtf8Bytes(body) : JsonSerializer.SerializeToUtf8Bytes(body, jsonTypeInfo);
            return BasicPublishAsync(exchange, routingKey, mandatory, basicProperties, b, cancellationToken);
        }
        public ValueTask BasicPublishAsync<TProperties>(string exchange, string routingKey,
            bool mandatory, TProperties? basicProperties, ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken = default)
            where TProperties : IReadOnlyBasicProperties, IAmqpHeader
        {
            
            return _initializer.BasicPublishAsync(exchange, routingKey, mandatory,basicProperties, body, cancellationToken);
        }
        public ValueTask BasicAckAsync(ulong deliveryTag, bool multiple,
            CancellationToken cancellationToken = default)
        {
            return _initializer.BasicAckAsync(deliveryTag, multiple, cancellationToken);
        }
        public ValueTask BasicNackAsync(ulong deliveryTag, bool multiple, bool requeue,
            CancellationToken cancellationToken = default)
        {
            return _initializer.BasicNackAsync(deliveryTag, multiple, requeue, cancellationToken);
        }
        public void BasicAcksAsync(AsyncEventHandler<BasicAckEventArgs> callback)
        {
            _initializer.SubscribeToBasicAcks(callback);
        }
        public void BasicNacksAsync(AsyncEventHandler<BasicNackEventArgs> callback)
        {
            _initializer.SubscribeToBasicNacks(callback);
        }


        public void SubscribeToBasicAcks(AsyncEventHandler<BasicAckEventArgs> callback) {
            _initializer.SubscribeToBasicAcks(callback);
        }
        public void SubscribeToBasicNacks(AsyncEventHandler<BasicNackEventArgs> callback) {
            _initializer.SubscribeToBasicNacks(callback);
        }
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
            await _initializer.DisposeAsync();
            if (connection != null) await connection.DisposeAsync().ConfigureAwait(false);
            _isDisposed = true;
        }
    }
}
