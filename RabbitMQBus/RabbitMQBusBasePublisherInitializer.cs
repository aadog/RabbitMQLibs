using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Threading;

namespace RabbitMQBus
{
    public abstract class RabbitMQBusBasePublisherInitializer(): IRabbitMQBusPublisherInitializer
    {
        protected bool _isDisposed;
        public ConcurrentDictionary<ulong, TaskCompletionSource<bool>> PublisherResultMap { get; }= new();
        public IChannel? Channel { get; private set; }

        public async Task createQueue(string queueName,bool dlx=false,int dlxTtl=5000,CancellationToken cancellationToken=default) {
            if (dlx)
            {
                var queueDlxName = $"{queueName}.dlx";
                var queueDlqName = $"{queueName}.dlq";
                var queueDlRouterKey = $"{queueName}.route.dlq";
                var queueMainRouterKey = $"{queueName}.route.main";
                await Channel!.ExchangeDeclareAsync(queueDlxName, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                var mainArgs = new Dictionary<string, object>()
                {
                    {"x-dead-letter-exchange", queueDlxName},
                    {"x-dead-letter-routing-key", queueDlRouterKey}
                };
                await Channel!.QueueDeclareAsync(queueName, true, false, false, arguments: mainArgs!, cancellationToken: cancellationToken).ConfigureAwait(false);
                var dlqArgs = new Dictionary<string, object>() {
                    {"x-message-ttl", dlxTtl},
                    {"x-dead-letter-exchange", queueDlxName}, 
                    // 消息会以主队列的名称作为路由键被重新投递
                    {"x-dead-letter-routing-key", queueMainRouterKey}
                };
                // 声明死信队列
                await Channel.QueueDeclareAsync(queueDlqName, true, false, false, arguments: dlqArgs!, cancellationToken: cancellationToken).ConfigureAwait(false);
                // 绑定死信队列到 DLX
                await Channel.QueueBindAsync(queueDlqName, queueDlxName, queueDlRouterKey, cancellationToken: cancellationToken).ConfigureAwait(false);
                //绑定到主队列
                await Channel.QueueBindAsync(queueName, queueDlxName, queueMainRouterKey, cancellationToken: cancellationToken).ConfigureAwait(false);
                await Channel.BasicQosAsync(0, 1, false, cancellationToken).ConfigureAwait(false);
            }
            else {
                await Channel!.QueueDeclareAsync(queueName, true, false, false, cancellationToken: cancellationToken).ConfigureAwait(false);
                await Channel.BasicQosAsync(0, 1, false, cancellationToken).ConfigureAwait(false);

            }
            
        }
        public void RegisterConfirmEvents()
        {
            // 监听确认事件
            Channel!.BasicAcksAsync += OnBasicAcks;
            // 监听未确认事件
            Channel!.BasicNacksAsync += OnBasicNacks;
        }

        public async Task OnBasicNacks(object sender, BasicNackEventArgs @event)
        {
            if (!@event.Multiple)
            {
                if (PublisherResultMap.TryGetValue(@event.DeliveryTag, out var tcs))
                {
                    tcs.TrySetResult(false);
                    PublisherResultMap.Remove(@event.DeliveryTag,out _); // 移除已确认的 Tag
                }
            }
            else
            {
                var tagsToRemove = PublisherResultMap.Keys
                    .Where(tag => tag <= @event.DeliveryTag)
                    .ToList();
                foreach (var tag in tagsToRemove)
                {
                    if (PublisherResultMap.TryGetValue(tag, out var tcs))
                    {
                        tcs.TrySetResult(false);
                        PublisherResultMap.Remove(tag,out _); // 移除已确认的 Tag
                    }
                }
            }
        }

        public virtual async Task OnBasicAcks(object sender, BasicAckEventArgs @event)
        {
            if (!@event.Multiple)
            {
                if (PublisherResultMap.TryGetValue(@event.DeliveryTag, out var tcs))
                {
                    tcs.TrySetResult(true);
                    PublisherResultMap.Remove(@event.DeliveryTag,out _); // 移除已确认的 Tag
                }
            }
            else
            {
                var tagsToRemove = PublisherResultMap.Keys
                    .Where(tag => tag <= @event.DeliveryTag)
                    .ToList();
                foreach (var tag in tagsToRemove)
                {
                    if (PublisherResultMap.TryGetValue(tag, out var tcs))
                    {
                        tcs.TrySetResult(true);
                        PublisherResultMap.Remove(tag,out _); // 移除已确认的 Tag
                    }
                }
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
            if (Channel != null) await Channel.DisposeAsync().ConfigureAwait(false);
        }
    }
}
