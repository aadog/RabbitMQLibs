using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace RabbitMQBus
{
    public abstract class RabbitMQBusBaseConsumerInitializer:IRabbitMQBusConsumerInitializer
    {
        protected bool _isDisposed;
        public List<IRabbitMQSubscription> _subscriptions { get; private set; }= new List<IRabbitMQSubscription>();

        public IReadOnlyCollection<IRabbitMQSubscription> Subscriptions => _subscriptions;
        // 提供线程安全的添加方法
        public void AddSubscription(IRabbitMQSubscription subscription)
        {
            lock (_subscriptions) {
                _subscriptions.Add(subscription);
            }
        }
        public bool RemoveSubscription(IRabbitMQSubscription subscription)
        {
            lock (_subscriptions)
            {
                return _subscriptions.Remove(subscription);
            }
        }
        public virtual async Task InitializeAsync(IConnection connection, CancellationToken cancellationToken)
        {
            foreach (var subscription in _subscriptions)
            {
                await subscription.InitializeAsync(connection, cancellationToken).ConfigureAwait(false);
            }
            await ConfigureResourcesAsync(cancellationToken).ConfigureAwait(false);
        }
        protected virtual Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            if (_isDisposed == true) return;
            foreach (var subscription in Subscriptions) {
                subscription.Dispose();
            }
            _isDisposed = true;
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (_isDisposed == true) return;
            foreach (var subscription in Subscriptions)
            {
                await subscription.DisposeAsync().ConfigureAwait(false);
            }
            _isDisposed = true;
        }
    }
}
