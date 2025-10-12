using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQBus;


namespace RabbitMQLibsTest
{
    public class PublisherInit():RabbitMQBusBasePublisherInitializer
    {
        public override Task InitializeAsync(string? Tag, IConnection connection, CancellationToken cancellationToken)
        {
            return base.InitializeAsync(Tag, connection, cancellationToken);
        }
        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            await Channel!.ExchangeDeclareAsync("zz", "direct", false, false);
        }
    }
}
