using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Common
{
    public class ClientInformation
    {
        private IPEndPoint _clientIP;
        private SocketAsyncEventArgs _asyncArg;
        private object _state;
        private Guid _id;

        public IPEndPoint ClientIP
        {
            get { return _clientIP; }
            set { _clientIP = value; }
        }

        public ClientInformation(IPEndPoint ip)
        {
            _clientIP = ip;
            _id = Guid.NewGuid();
        }

        public object State
        {
            get { return _state; }
            set { _state = value; }
        }

        public string ID { get { return _id.ToString(); } }
    }
}
