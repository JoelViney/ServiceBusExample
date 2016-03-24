using ServiceBusExample.SimpleQueues.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBusExample.SimpleQueues
{
    /// <summary>
    /// This is the core of the Queues.
    /// It is used to process the messages from the queues. 
    /// It is called from the Azure Queue WebJob.
    /// </summary>
    public class QueueProcessor
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static long ProcessMaxMessages = 5;

        private IQueueConsumer queueConsumer;

        #region Constructors...

        public QueueProcessor()
        {
            this.queueConsumer = new QueueConsumer();
        }

        public QueueProcessor(IQueueConsumer queueConsumer)
        {
            this.queueConsumer = queueConsumer;
        }

        #endregion

        public async Task HeartbeatAsync()
        {
            Log.Info("Starting HeartbeatAsync...");

            await ProcessQueueAsync<TestQueueMessage>();

            Log.Info("Starting Done...");
        }

        /// <summary>
        /// Processes a single message type. This should only be used in unit tests.
        /// </summary>
        public async Task HeartbeatAsync<T>() where T : QueueMessageBase
        {
            await ProcessQueueAsync<T>();
        }
        
        private async Task ProcessQueueAsync<T>() where T : QueueMessageBase
        {
            Log.DebugFormat("Processing messages for {0}...", typeof(T).Name);

            using (QueueManager<T> queueManager = new QueueManager<T>())
            {
                long count = queueManager.GetCount();

                if (count > ProcessMaxMessages)
                {
                    count = ProcessMaxMessages;
                }

                Log.InfoFormat("Processing {0} messages for {1}.", count, typeof(T).Name);

                for (int i = 0; i < count; i++)
                {
                    T item = queueManager.Receive();

                    if (item != null)
                    {
                        try
                        {
                            await queueConsumer.ProcessAsync(item);
                            queueManager.Complete(item);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("The Queue Consumer failed to process the message.", ex);
                            // Re-add the message to the end of the Queue...
                            item.Attempts += 1;

                            if (item.Attempts < 3)
                            {
                                // Make 3 attempts to deliver the message.
                                // Re-add the message to the end of the Queue...
                                queueManager.MoveToEnd(item);
                            }
                            else
                            {
                                queueManager.MoveToDeadLetter(item);
                            }
                        }
                    }
                }
            }

            Log.DebugFormat("Processing messages for {0}. Done", typeof(T).Name);
        }
        
    }
}
