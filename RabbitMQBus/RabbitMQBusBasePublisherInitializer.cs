using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQBus
{
    public abstract class RabbitMQBusBasePublisherInitializer(): IRabbitMQBusPublisherInitializer
    {
        protected bool _isDisposed;
        public IChannel? Channel { get; private set; }

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
