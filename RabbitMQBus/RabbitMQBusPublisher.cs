using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQCommon;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace RabbitMQBus
{
    public class RabbitMQBusPublisher<TRabbitMQBusPublisherInitializer>(TRabbitMQBusPublisherInitializer initializer) : IRabbitMqComponent
        where TRabbitMQBusPublisherInitializer : class,IRabbitMQBusPublisherInitializer
    {
        public string? ClientProvidedName { get; set; }
        public bool IsStarted { get; set; }
        public IConnection Connection => _connection!;
        public string? Tag { get; set; }
        private IConnection? _connection { get; set; } = null;
        public TRabbitMQBusPublisherInitializer Initializer => initializer;

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
            
            return initializer.BasicPublishAsync(exchange, routingKey, mandatory,basicProperties, body, cancellationToken);
        }
        public ValueTask BasicAckAsync(ulong deliveryTag, bool multiple,
            CancellationToken cancellationToken = default)
        {
            return initializer.BasicAckAsync(deliveryTag, multiple, cancellationToken);
        }
        public ValueTask BasicNackAsync(ulong deliveryTag, bool multiple, bool requeue,
            CancellationToken cancellationToken = default)
        {
            return initializer.BasicNackAsync(deliveryTag, multiple, requeue, cancellationToken);
        }
        public void BasicAcksAsync(AsyncEventHandler<BasicAckEventArgs> callback)
        {
            initializer.SubscribeToBasicAcks(callback);
        }
        public void BasicNacksAsync(AsyncEventHandler<BasicNackEventArgs> callback)
        {
            initializer.SubscribeToBasicNacks(callback);
        }


        public void SubscribeToBasicAcks(AsyncEventHandler<BasicAckEventArgs> callback) {
            initializer.SubscribeToBasicAcks(callback);
        }
        public void SubscribeToBasicNacks(AsyncEventHandler<BasicNackEventArgs> callback) {
            initializer.SubscribeToBasicNacks(callback);
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
