using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusExample.SimpleQueues
{
    /// <summary>
    /// Defines all of the application settings used by the Queues.
    /// </summary>
    public class QueueSettings : ConfigurationSection
    {
        public QueueSettings()
        {
            try
            {
                NameValueCollection settingCollection = (NameValueCollection)ConfigurationManager.GetSection("queueSettings");

                this.ConnectionString = settingCollection["ConnectionString"];
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to load the settings for the Queue.", ex);
            }
        }

        /// <summary>
        /// The connection string to the Azure ServiceBus queues.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
