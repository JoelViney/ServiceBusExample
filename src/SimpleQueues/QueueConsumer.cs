using ServiceBusExample.SimpleQueues.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServiceBusExample.SimpleQueues
{
    /// <summary>
    /// Provides an interface for 'something' that consumes or acts upon the output from the QueueController.
    /// This is interfaces so we can stub it out in integration tests.
    /// </summary>
    public interface IQueueConsumer
    {
        Task ProcessAsync(QueueMessageBase message);
    }

    public class QueueConsumer : IQueueConsumer
    {
        private QueueSettings Settings { get; set; }

        public QueueConsumer()
        {
            this.Settings = new QueueSettings();
        }

        public async Task ProcessAsync(QueueMessageBase message)
        {
            if (message == null)
            {
                throw new QueueConsumerException("A null type was passed to the QueueConsumer.");
            }

            if (message is TestQueueMessage)
            {
                await Task.Run(() => Process(message));
            }
            else
            {
                Type type = message.GetType();
                throw new QueueConsumerException("Invalid type passed to the QueueConsumer: " + type.Name);
            }
        }

        public void Process(QueueMessageBase message)
        {
            // TODO: Do something with a database, URL etc...
            Type type = message.GetType();
            throw new QueueConsumerException("Processing message: " + type.Name);
        }
    }
}
