using RabbitMQ.Client;
using RabbitMQBus;

namespace RabbitMQLibsTest
{
    public class ConsumerInit:RabbitMQBusBaseConsumerInitializer
    {
        public override Task InitializeAsync(IConnection connection, CancellationToken cancellationToken)
        {
            AddSubscription(new TestSubscription());
            return base.InitializeAsync(connection, cancellationToken);
        }
    }
}
