using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQBus
{
    public interface IRabbitMQSubscription:IDisposable,IAsyncDisposable
    {
        public IChannel? Channel { get; set; }
        public CreateChannelOptions? CreateChannelOptions { get; }
        string QueueName { get; }
        protected virtual bool AutoAck => false;
        public Task InitializeAsync(IConnection connection, CancellationToken cancellationToken);
        public Task HandleMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken);

    }
}
