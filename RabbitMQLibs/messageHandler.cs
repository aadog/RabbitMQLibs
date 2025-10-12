using RabbitMQBus;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQLibsTest
{
    internal class messageHandler(IConnectionFactory connectionFactory) :IMessageHandler
    {
        public Task HandleAsync(BasicDeliverEventArgs args, IChannel channel, CancellationToken cancellationToken)
        {
            Console.WriteLine(connectionFactory);
            return Task.CompletedTask;
        }
    }
}
