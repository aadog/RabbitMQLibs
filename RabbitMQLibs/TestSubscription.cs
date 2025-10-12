using RabbitMQ.Client.Events;
using RabbitMQBus;
using System.Text;

namespace RabbitMQLibsTest
{
    public class TestSubscription:RabbitMQBusBaseSubscription
    {
        public override string QueueName { get; } = "bb";
        protected override bool AutoAck => false;

        protected override async Task ConfigureResourcesAsync(CancellationToken cancellationToken)
        {
            await Channel!.QueueDeclareAsync("bb", false, false, false);
            await Channel!.QueueBindAsync("bb", "zz", "test");
        }
        public override async Task HandleMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            await Task.Delay(1000);
            //var a = Encoding.UTF8.GetString(args.Body.Span);
            Console.WriteLine($"收到消息"); // 每秒调用数万次
            await Channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
            //Thread.Sleep(100); // 模拟轻微延迟
            //Console.WriteLine($"收到消息:{Encoding.UTF8.GetString(args.Body.ToArray())}");
        }
    }
}
