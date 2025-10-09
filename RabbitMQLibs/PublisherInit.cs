using RabbitMQBus;


namespace RabbitMQLibsTest
{
    public class PublisherInit:RabbitMQBusBasePublisherInitializer
    {
        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            await Channel!.ExchangeDeclareAsync("zz", "direct", false, false);
        }
    }
}
