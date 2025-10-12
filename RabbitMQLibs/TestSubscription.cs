using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQBus;
using System.Text;

namespace RabbitMQLibsTest
{
    public class TestSubscription(IServiceProvider serviceProvider):RabbitMQBusBaseSubscription(serviceProvider)
    {
        public override string QueueName { get; set; } = "bb";
        public override bool AutoAck => true;

        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            await Channel!.QueueDeclareAsync("bb", false, false, false);
            await Channel!.QueueBindAsync("bb", "zz", "test");
        }
        public override async Task HandleMessageAsync(BasicDeliverEventArgs args,IChannel channel, CancellationToken cancellationToken)
        {
            await CreateMessageHandlerFuncAsync<messageHandler>()(args, channel, cancellationToken);
        }
    }
}
