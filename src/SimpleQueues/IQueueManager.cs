using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusExample.SimpleQueues
{
    public interface IQueueManager<T> : IDisposable
    {
        /// <summary>Adds the message to the Queue.</summary>
        void Send(T message);

        /// <summary>
        /// Gets the message and flags the message as locked (so no one else can see it for 1 minute). 
        /// The Receive is a two stage operation, the message is only removed from the queue if Complete() is called.
        /// </summary>
        T Receive();

        /// <summary>
        /// Marks the object as processed and removes it from the queue.
        /// </summary>
        T Complete(T message);

        /// <summary>
        /// Receives the item and moves it to the back / end of the Queue.
        /// </summary>
        void MoveToEnd(T message);

        long GetCount();

        void ClearQueue();
    }

}
