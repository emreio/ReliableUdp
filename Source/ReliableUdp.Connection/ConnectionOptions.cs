using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
namespace ReliableUdp.Connection
{
    public class ConnectionOptions
    {
        public int RecieveBufferSize { get; set; }
        public int SendBufferSize { get; set; }
        public int ListeningPortNumber { get; set; }
        public int AckListeningPortNumber { get; set; }
        public bool Listen { get; set; }
        public bool Broadcast { get; set; }
        public bool LoopBack { get; set; }
        public int RecieveTimeout { get; set; }
        public int SendTimeout { get; set; }
        public NetworkInterfaceType InterfaceType { get; set; }
    }
}
