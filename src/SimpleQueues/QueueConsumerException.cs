using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBusExample.SimpleQueues
{
    /// <summary>
    /// Used by the QueueConsumer
    /// </summary>
    public class QueueConsumerException : ApplicationException
    {
        public QueueConsumerException()
        {

        }

        public QueueConsumerException(string message) : base(message)
        {

        }
    }
}
