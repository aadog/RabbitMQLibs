using RabbitMQ.Client;
using RabbitMQBus;
using System.Diagnostics.CodeAnalysis;

namespace RabbitMQLibsTest
{
    public class ConsumerInit(IServiceProvider serviceProvider) : RabbitMQBusBaseConsumerInitializer(serviceProvider)
    {
        public override Task InitializeAsync(string? Tag, IConnection connection, CancellationToken cancellationToken)
        {
            AddSubscription(CreateSubscription<TestSubscription>());
            return base.InitializeAsync(Tag, connection, cancellationToken);
        }
    }
}
