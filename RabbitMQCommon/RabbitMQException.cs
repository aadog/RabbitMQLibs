using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQCommon
{
    public class RabbitMQException(string message):Exception(message)
    {
    }
}
