using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusExample.SimpleQueues.Messages
{
    /// <summary>
    /// This is what is returned from the API.. 
    /// </summary>
    public class MessageResponse
    {
        /// <summary>
        /// Returns true if the opertaion that was called was a success.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If the operation failed it returns the details as to why it failed.
        /// </summary>
        public string Message { get; set; }

        public MessageResponse()
        {
            Success = true;
            Message = null;
        }
    }
}
