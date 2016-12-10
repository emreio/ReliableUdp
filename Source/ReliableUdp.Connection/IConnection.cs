using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Connection
{
    public interface IUdpConnection : IDisposable
    {
        void Bind(IPEndPoint localEndPoint);
        void Connect(IPEndPoint remote);
        void SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remote);
        bool SendToAsync(SocketAsyncEventArgs arguments);
        int RecieveFrom(byte[] buffer, ref EndPoint remoteEndpoint);
        bool RecieveFromAsync(SocketAsyncEventArgs arguments);
    }
}
