using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQBus
{
    public interface IMessageHandler
    {
        public Task HandleAsync(BasicDeliverEventArgs args,IChannel channel,CancellationToken cancellationToken);
    }
}
