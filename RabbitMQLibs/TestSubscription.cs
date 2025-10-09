using RabbitMQ.Client.Events;
using RabbitMQBus;

namespace RabbitMQLibsTest
{
    public class TestSubscription:RabbitMQBusBaseSubscription
    {
        public override string QueueName { get; } = "bb";
        protected override bool AutoAck => true;

        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            await Channel!.QueueDeclareAsync("bb", false, false, false);
            await Channel!.QueueBindAsync("bb", "zz", "test");
        }
        public override Task HandleMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            Console.WriteLine("zz");
            return Task.CompletedTask;
        }
    }
}
