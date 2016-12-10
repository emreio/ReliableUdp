using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ReliableUdp.Connection
{
    public class SocketClientContext
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public SocketAsyncEventArgs SocketState { get; set; }
    }
}
