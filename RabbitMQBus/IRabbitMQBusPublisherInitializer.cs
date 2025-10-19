using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQBus
{
    public interface IRabbitMQBusPublisherInitializer:IDisposable,IAsyncDisposable
    {
        public IChannel? Channel { get; }
        public CreateChannelOptions? CreateChannelOptions { get;}
        public Task InitializeAsync(string? Tag,IConnection connection, CancellationToken cancellationToken);
    }
}
