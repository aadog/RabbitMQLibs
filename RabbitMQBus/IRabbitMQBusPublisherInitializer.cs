using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQBus
{
    public interface IRabbitMQBusPublisherInitializer:IDisposable,IAsyncDisposable
    {
        public void SubscribeToBasicAcks(AsyncEventHandler<BasicAckEventArgs> callback);
        public void SubscribeToBasicNacks(AsyncEventHandler<BasicNackEventArgs> callback);
        public ValueTask BasicNackAsync(ulong deliveryTag, bool multiple, bool requeue,
            CancellationToken cancellationToken = default);
        public ValueTask BasicAckAsync(ulong deliveryTag, bool multiple,
            CancellationToken cancellationToken = default);
        public ValueTask BasicPublishAsync<TProperties>(string exchange, string routingKey,
            bool mandatory, TProperties? basicProperties, ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken = default) where TProperties : IReadOnlyBasicProperties, IAmqpHeader;
        public CreateChannelOptions? CreateChannelOptions { get;}
        public Task InitializeAsync(IConnection connection, CancellationToken cancellationToken);
    }
}
