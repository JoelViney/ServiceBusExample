using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusExample.SimpleQueues.Messages
{
    public class RedWidget : QueueMessageBase
    {
        public string Name { get; set; }

        public RedWidget() : base()
        {
            this.Name = Guid.NewGuid().ToString();
        }

        public override bool Equals(object obj)
        {
            var item = obj as RedWidget;

            if (item == null)
            {
                return false;
            }

            return this.Name.Equals(item.Name);
        }


        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
