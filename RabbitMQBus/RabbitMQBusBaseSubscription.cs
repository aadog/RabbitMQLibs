using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace RabbitMQBus
{
    public abstract class RabbitMQBusBaseSubscription(IServiceProvider serviceProvider):IRabbitMQSubscription
    {
        protected IServiceProvider ServiceProvider { get; } = serviceProvider;
        protected bool _isDisposed;
        public IChannel Channel => _channel!;
        private IChannel? _channel = null;
        public virtual CreateChannelOptions? CreateChannelOptions { get; protected set; } = new CreateChannelOptions(false, false, consumerDispatchConcurrency: 1);
        public abstract string QueueName { get; }
        protected virtual bool AutoAck => false;


        // 创建通道（可被继承类重写）
        public virtual async Task<IChannel> CreateChannelAsync(IConnection connection, CancellationToken cancellationToken)
        {
            return await connection.CreateChannelAsync(CreateChannelOptions, cancellationToken).ConfigureAwait(false); ;
        }
        public virtual async Task InitializeAsync(IConnection connection, CancellationToken cancellationToken) {
            _channel = await CreateChannelAsync(connection, cancellationToken).ConfigureAwait(false);
            // 配置资源（交换机、队列、绑定等）
            await Channel.BasicQosAsync(0, 1, false, cancellationToken).ConfigureAwait(false);

            await ConfigureResourcesAsync(cancellationToken).ConfigureAwait(false);
            await RegisterMessageHandlerAsync(cancellationToken).ConfigureAwait(false);
        }
        // 消息处理方法 - 由子类实现，处理业务逻辑
        public abstract Task HandleMessageAsync(BasicDeliverEventArgs args,IChannel channel, CancellationToken cancellationToken);
        // 注册消息处理器 - 基类封装通用逻辑
        protected virtual async Task RegisterMessageHandlerAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += async (sender, args) =>
            {

                // 将完整消息上下文传递给子类处理
                await HandleMessageAsync(args,Channel, cancellationToken).ConfigureAwait(false);
            };
            // 使用AutoAck属性控制确认模式
            await Channel.BasicConsumeAsync(QueueName, autoAck: AutoAck, consumer: consumer, cancellationToken).ConfigureAwait(false);

        }
        public Func<BasicDeliverEventArgs,IChannel,CancellationToken,Task> CreateMessageHandlerFuncAsync<TMessageHandler>(params object[] parameters) 
            where TMessageHandler : IMessageHandler
        {
            var scope=ServiceProvider.CreateScope();
            var sp = scope.ServiceProvider;
            var handler = (TMessageHandler)ActivatorUtilities.CreateInstance(sp, typeof(TMessageHandler), parameters);
            return handler.HandleAsync;
        }
        // 配置通知相关的资源
        protected virtual async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            
            await Task.CompletedTask;
        }
        public virtual void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _channel?.Dispose();
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            if (_channel != null) {
                await _channel.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
