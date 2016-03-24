using ServiceBusExample.SimpleQueues.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusExample.SimpleQueues.Stubs
{
    public class QueueConsumerStub : IQueueConsumer
    {
        public IList<QueueMessageBase> ProcessedMessages { get; set; }

        private bool _throwException { get; set; }

        public QueueConsumerStub()
        {
            this.ProcessedMessages = new List<QueueMessageBase>();
            this._throwException = false;
        }

        public QueueConsumerStub ThrowsException()
        {
            this._throwException = true;
            return this;
        }

        public async Task ProcessAsync(QueueMessageBase widget)
        {
            await Task.Run(() =>
            {
                if (this._throwException)
                {
                    throw new ApplicationException("The Consumer Stub was told to fail.");
                }

                AddMessage(widget);
            });
        }

        private void AddMessage(QueueMessageBase message)
        {
            ProcessedMessages.Add(message);
        }
    }
}
