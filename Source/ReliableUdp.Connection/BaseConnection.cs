using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Threading;

namespace ReliableUdp.Connection
{
    public abstract class BaseConnection : IUdpConnection
    {
        public Socket socket = null;
        protected bool isListening = false;
        protected readonly ConnectionOptions options = null;
        protected byte[] buffer = null;
        private bool isInitialized = false;

        IPEndPoint localEndPoint;

        bool isBroadcast = false;

        public BaseConnection(ConnectionOptions options)
        {
            this.options = options;

            Initialize();

            this.isInitialized = true;
        }

        private void Initialize()
        {
            CheckArguments(this.options);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            isBroadcast = options.Broadcast;

            AssignLocalEndPoint();

            Console.WriteLine(localEndPoint.ToString());

            if (this.options.Listen)
                Bind(localEndPoint);

            if (this.options.Broadcast)
                GrantBroadcast();
        }

        private void AssignLocalEndPoint()
        {
            IPAddress localIP = null;

            if (!options.LoopBack)
            {
                localIP = FindLocalIP(options.InterfaceType);
            }

            else
            {
                localIP = IPAddress.Loopback;
            }

            if (localIP != null)
                localEndPoint = new IPEndPoint(localIP, options.ListeningPortNumber);
        }

        private void CheckArguments(ConnectionOptions connectionOptions)
        {
            if (connectionOptions.Listen && connectionOptions.ListeningPortNumber == 0)
                throw new ArgumentException("Invalid port number.");
        }

        public void SetLocalEndPoint(IPEndPoint endpoint)
        {
            this.localEndPoint = endpoint;
        }

        protected virtual IPAddress FindLocalIP(NetworkInterfaceType type)
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) return null;

            var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var @interface in allNetworkInterfaces)
            {
                if (@interface.OperationalStatus == OperationalStatus.Up && @interface.NetworkInterfaceType == type)
                {
                    foreach (UnicastIPAddressInformation ip in @interface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            return ip.Address;
                    }
                }
            }

            return null;
        }

        #region IConnection Implementation

        public virtual void Bind(IPEndPoint localEndPoint)
        {
            //if (!isInitialized) throw new InvalidOperationException("Connection is not initialized.");

            if (localEndPoint != null)
                AssignLocalEndPoint();

            if (localEndPoint == null)
                throw new ApplicationException("Unable to find a local endpoint");

            if (socket == null)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Bind(localEndPoint);
        }

        public virtual void Connect(IPEndPoint remote)
        {
            if (socket != null)
            {
                socket.Connect(remote);
            }
        }

        public virtual void SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remote)
        {
            socket.SendTo(buffer, socketFlags, remote);
        }

        public virtual bool SendToAsync(SocketAsyncEventArgs arguments)
        {
            return this.socket.SendToAsync(arguments);
        }

        /// <summary>
        /// Sokete broadcast mesaj gönderme yetkisi veriliyor.
        /// </summary>
        public void GrantBroadcast()
        {
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        }

        public virtual int RecieveFrom(byte[] buffer, ref EndPoint remoteEndpoint)
        {
            return this.socket.ReceiveFrom(buffer, ref remoteEndpoint);
        }

        public virtual bool RecieveFromAsync(SocketAsyncEventArgs arguments)
        {
            return this.socket.ReceiveFromAsync(arguments);
        }

        public void Dispose()
        {
            if (socket != null)
            {
                socket.Close();
                socket.Dispose();
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get
            {
                return localEndPoint;
            }
        }

        #endregion
    }
}
