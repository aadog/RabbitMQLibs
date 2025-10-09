using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQBus
{
    public abstract class RabbitMQBusBaseSubscription:IRabbitMQSubscription,IDisposable,IAsyncDisposable
    {
        protected bool _isDisposed;
        public IChannel? Channel { get; set; }
        public virtual CreateChannelOptions? CreateChannelOptions { get; protected set; }
        public abstract string QueueName { get; }
        protected virtual bool AutoAck => false;


        // 创建通道（可被继承类重写）
        public virtual async Task<IChannel> CreateChannelAsync(IConnection connection, CancellationToken cancellationToken)
        {
            return await connection.CreateChannelAsync(CreateChannelOptions, cancellationToken).ConfigureAwait(false); ;
        }
        public virtual async Task InitializeAsync(IConnection connection, CancellationToken cancellationToken) { 
            Channel= await CreateChannelAsync(connection, cancellationToken).ConfigureAwait(false);
            // 配置资源（交换机、队列、绑定等）
            await ConfigureResourcesAsync(cancellationToken).ConfigureAwait(false);
            await RegisterMessageHandlerAsync(cancellationToken).ConfigureAwait(false);
        }
        // 消息处理方法 - 由子类实现，处理业务逻辑
        public abstract Task HandleMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken);
        // 注册消息处理器 - 基类封装通用逻辑
        protected virtual async Task RegisterMessageHandlerAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(Channel!);
            consumer.ReceivedAsync += async (sender, args) =>
            {
                
                // 将完整消息上下文传递给子类处理
                await HandleMessageAsync(args, cancellationToken).ConfigureAwait(false);
            };
           

            // 使用AutoAck属性控制确认模式
            await Channel!.BasicConsumeAsync(QueueName, autoAck: AutoAck, consumer: consumer).ConfigureAwait(false);
        }
        // 配置通知相关的资源
        protected virtual async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
        public virtual void Dispose()
        {
            if (_isDisposed) return;
            Channel?.Dispose();
            _isDisposed = true;
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;

            if (Channel != null) {
                await Channel.DisposeAsync().ConfigureAwait(false);
            }
            _isDisposed = true;
        }
    }
}
