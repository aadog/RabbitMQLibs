
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace RabbitMQBus
{
    public interface IRabbitMQBusConsumerInitializer : IDisposable, IAsyncDisposable
    {
        public IReadOnlyCollection<IRabbitMQSubscription> Subscriptions { get;  }
        public void AddSubscription(IRabbitMQSubscription subscription);
        public bool RemoveSubscription(IRabbitMQSubscription subscription);
        public Task InitializeAsync(IConnection connection, CancellationToken cancellationToken);
    }
}
