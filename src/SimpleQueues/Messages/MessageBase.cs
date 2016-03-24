using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServiceBusExample.SimpleQueues.Messages
{
    public abstract class QueueMessageBase
    {
        /// <summary>The maximum size of the message in bytes. 256kb - 64kb. The headers maximum size is 64kb so 192 is a safe number.</summary>
        public const int MaximumMessageSize = 192000;

        /// <summary>The number of times we have attempted to deliver the message.</summary>
        public int Attempts { get; set; }

        /// <summary>Used internally to keep track of the message lock without keeping a reference to the Actual BrokeredMessage</summary>
        internal Guid LockToken { get; set; }

        public QueueMessageBase()
        {

        }

    }
}
