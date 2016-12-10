using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Transmission
{
    internal class StreamBufferItem
    {
        private ulong _itemID;
        private byte[] _itemData;

        public StreamBufferItem(ulong id, byte[] data)
        {
            _itemID = id;
            _itemData = data;
        }

        public ulong ID
        {
            get { return _itemID; }
        }

        public byte[] Data
        {
            get { return _itemData; }
        }
    }
}
