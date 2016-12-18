using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ReliableUdp.Transmission
{
    internal class StreamContext
    {
        static StreamContext instance = new StreamContext();

        private List<StreamBufferItem> bag = new List<StreamBufferItem>();

        public static StreamContext Instance
        {
            get
            {
                return instance;
            }
        }

        public void Push(StreamBufferItem item)
        {
            bag.Add(item);
            
            if (bag.Count > 100)
                bag.RemoveRange(0, 1);
        }

        public StreamBufferItem Find(ulong id)
        {
            return bag.Where(x => x.ID == id).FirstOrDefault();
        }

        private StreamContext()
        {

        }
    }

}
