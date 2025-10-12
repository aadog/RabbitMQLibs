using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQBus
{
    public abstract class RabbitMQBusBasePublisherInitializer(): IRabbitMQBusPublisherInitializer
    {
        protected bool _isDisposed;
        protected IChannel? Channel { get; private set; }
        public ValueTask BasicPublishAsync<TProperties>(string exchange, string routingKey, bool mandatory,
            TProperties? basicProperties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default)
            where TProperties : IReadOnlyBasicProperties, IAmqpHeader
        {
            if (basicProperties == null)
            {
                return Channel!.BasicPublishAsync(exchange, routingKey, body: body, mandatory: mandatory, cancellationToken: cancellationToken);
            }
            else {
                return Channel!.BasicPublishAsync(exchange, routingKey, body: body, mandatory: mandatory, basicProperties: basicProperties, cancellationToken: cancellationToken);
            }
            
        }

        public virtual CreateChannelOptions? CreateChannelOptions { get; private set; } = null;
        // 配置资源 - 由子类实现，定义交换机、队列等
        protected virtual Task ConfigureResourcesAsync(CancellationToken cancellationToken) { 
            return Task.CompletedTask;
        }

        public virtual async Task InitializeAsync(string? Tag, IConnection connection, CancellationToken cancellationToken)
        {
            // 创建通道
            Channel = await CreateChannelAsync(connection, cancellationToken).ConfigureAwait(false);
            await ConfigureResourcesAsync(cancellationToken).ConfigureAwait(false);
        }
        // 创建通道（可被继承类重写）
        protected virtual async Task<IChannel> CreateChannelAsync(IConnection connection, CancellationToken cancellationToken)
        {
            return await connection.CreateChannelAsync(CreateChannelOptions, cancellationToken).ConfigureAwait(false);
        }




        public ValueTask BasicAckAsync(ulong deliveryTag, bool multiple,
            CancellationToken cancellationToken = default)
        { 
            return Channel!.BasicAckAsync(deliveryTag, multiple, cancellationToken);
        }

        public void SubscribeToBasicAcks(AsyncEventHandler<BasicAckEventArgs> callback)
        {
            Channel!.BasicAcksAsync += callback;
        }

        public void SubscribeToBasicNacks(AsyncEventHandler<BasicNackEventArgs> callback)
        {
            Channel!.BasicNacksAsync += callback;
        }

        public ValueTask BasicNackAsync(ulong deliveryTag, bool multiple, bool requeue,
            CancellationToken cancellationToken = default)
        {
            return Channel!.BasicNackAsync(deliveryTag, multiple,requeue, cancellationToken);
        }




        public  virtual void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Channel?.Dispose();
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            if (Channel != null) await Channel.DisposeAsync();
        }
    }
}
