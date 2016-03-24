using ServiceBusExample.SimpleQueues.Messages;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace ServiceBusExample.SimpleQueues
{
    /// <summary>
    /// This provides the interface to the Queues.
    /// 
    /// It is used by the parts of the application that need to add stuff to the queues such as the Scheduler.
    /// and is also used by the QueueController to pull messages off the queue and process them.
    /// </summary>
    public class QueueManager<T> : IQueueManager<T>, IDisposable
        where T : QueueMessageBase
    {
        protected const int MaximumSessionSize = 1000;

        private NamespaceManager _namespaceManager;
        private QueueClient _queueClient;

        protected IQueueManager<T> _deadLetterQueue;

        protected string QueueName { get; private set; }


        #region Constructors / Destructors...

        public QueueManager()
        {
            Type type = typeof(T);
            this.QueueName = type.Name.ToLower();

        }

        // This is used when constucting dead letter queues.
        protected QueueManager(string queueName)
        {
            this.QueueName = queueName;
        }

        public void Dispose()
        {
            if (this._queueClient != null)
                if (!this.QueueClient.IsClosed)
                    this.QueueClient.Close();

            if (this._deadLetterQueue != null)
                this._deadLetterQueue.Dispose();
        }

        #endregion

        public IQueueManager<T> DeadLetters
        {
            get
            {
                if (_deadLetterQueue == null)
                {
                    string deadLetterQueueName = QueueClient.FormatDeadLetterPath(this.QueueClient.Path);
                    this._deadLetterQueue = new QueueManager<T>(deadLetterQueueName);
                }
                return this._deadLetterQueue;
            }
        }


        public void Send(T message)
        {
            BrokeredMessage brokeredMessage = new BrokeredMessage(message);
            this.QueueClient.Send(brokeredMessage);
        }

        public T Receive()
        {
            T item = null;
            BrokeredMessage brokeredMessage = this.QueueClient.Receive(TimeSpan.FromSeconds(5));

            // Single Message
            item = brokeredMessage.GetBody<T>();

            item.LockToken = brokeredMessage.LockToken;

            return item;
        }

        public T Complete(T message)
        {
            this.QueueClient.Complete(message.LockToken);

            return message;
        }

        public void MoveToEnd(T message)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                this.Complete(message);
                this.Send(message);
                scope.Complete();
            }
        }

        public void MoveToDeadLetter(T message)
        {
            this.QueueClient.DeadLetter(message.LockToken);
        }

        public void ClearQueue()
        {
            long count = this.GetCount();

            // Batch the receive operation
            while (count > 0)
            {
                IEnumerable<BrokeredMessage> brokeredMessages = null;
                if (count > 100)
                    brokeredMessages = this.QueueClient.ReceiveBatch(100);
                else
                    brokeredMessages = this.QueueClient.ReceiveBatch((int)count);

                // Complete the messages
                var completeTasks = brokeredMessages.Select(m => Task.Run(() => m.Complete())).ToArray();

                // Wait for the tasks to complete. 
                Task.WaitAll(completeTasks);

                count = count - 100;
            }
        }



        public long GetCount()
        {
            AssureQueueExists();
            long count;

            if (!this.IsDeadLetterQueue())
            {
                count = this.NamespaceManager.GetQueue(this.QueueName).MessageCountDetails.ActiveMessageCount;
            }
            else
            {
                string queueName = this.QueueName;

                queueName = queueName.Substring(0, queueName.Length - "/$DeadLetterQueue".Length);
                count = this.NamespaceManager.GetQueue(queueName).MessageCountDetails.DeadLetterMessageCount;
            }

            return count;
        }


        #region Helper Methods...

        /// <summary>
        /// Retrieves the Azure ServiceBus connection string from the Queue settings
        /// </summary>
        private string ConnectionString
        {
            get
            {
                QueueSettings settings = new QueueSettings();
                return settings.ConnectionString;
            }
        }

        private NamespaceManager NamespaceManager
        {
            get
            {
                if (this._namespaceManager == null)
                {
                    _namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
                }
                return _namespaceManager;
            }
        }

        protected QueueClient QueueClient
        {
            get
            {
                if (this._queueClient == null)
                {
                    AssureQueueExists();

                    _queueClient = QueueClient.CreateFromConnectionString(ConnectionString, this.QueueName);
                }

                return _queueClient;
            }
        }

        private void AssureQueueExists()
        {
            if (this.IsDeadLetterQueue())
                return;

            if (!this.NamespaceManager.QueueExists(this.QueueName))
            {
                QueueDescription desc = new QueueDescription(this.QueueName);
                this.NamespaceManager.CreateQueue(desc);
            }
        }

        protected bool IsDeadLetterQueue()
        {
            return this.QueueName.EndsWith("/$DeadLetterQueue");
        }

        #endregion
    }
}
