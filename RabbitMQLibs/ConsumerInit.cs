using RabbitMQ.Client;
using RabbitMQBus;

namespace RabbitMQLibsTest
{
    public class ConsumerInit(IServiceProvider serviceProvider) :RabbitMQBusBaseConsumerInitializer(serviceProvider)
    {
        public override Task InitializeAsync(IConnection connection, CancellationToken cancellationToken)
        {
            AddSubscription(CreateSubscription<TestSubscription>());
            return base.InitializeAsync(connection, cancellationToken);
        }
    }
}
