using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace RabbitMQBus
{
    public interface IRabbitMQBusPublisherInitializer:IDisposable,IAsyncDisposable
    {
        public ConcurrentDictionary<ulong, TaskCompletionSource<bool>> PublisherResultMap { get; }
        public IChannel? Channel { get; }
        public CreateChannelOptions? CreateChannelOptions { get;}
        public Task InitializeAsync(string? Tag,IConnection connection, CancellationToken cancellationToken);
    }
}
