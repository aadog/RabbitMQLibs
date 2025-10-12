using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQBus
{
    public interface IRabbitMQSubscription:IDisposable,IAsyncDisposable
    {
        public IChannel? Channel { get;}
        public CreateChannelOptions? CreateChannelOptions { get; }
        string QueueName { get; set; }
        public bool AutoAck { get; set; }
        public Task InitializeAsync(IConnection connection, CancellationToken cancellationToken);
        public Task HandleMessageAsync(BasicDeliverEventArgs args,IChannel channel, CancellationToken cancellationToken);

    }
}
