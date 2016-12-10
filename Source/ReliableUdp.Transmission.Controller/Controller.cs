using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using System.Net;
using ReliableUdp.Common;
using ReliableUdp.Transmission.Utilities;
using System.Net.Sockets;

namespace ReliableUdp.Transmission
{
    public class Controller
    {
        private static readonly IDictionary<string, ClientInformation> clients = new Dictionary<string, ClientInformation>();

        StreamContext streamContext = StreamContext.Instance;

        private static readonly object lockObj = new object();

        private ManualResetEvent listenFlag = new ManualResetEvent(false);
        private ManualResetEvent streamFlag = new ManualResetEvent(false);

        byte[] ackRecieveBuffer = new byte[65536];

        private IPEndPoint ackEndpoint = new IPEndPoint(IPAddress.Any, 0);

        Connection.Connection socket;
        Connection.ConnectionOptions options;
        Thread listenigThread;
        Thread streamingThread;

        public Controller()
        {
            LoadOptions();

            socket = new Connection.Connection(options);
        }

        protected virtual void AcceptClient(ClientInformation client)
        {
            lock (lockObj)
            {
                client.ClientIP = client.ClientIP;

                if (clients.ContainsKey(client.ID))
                    clients.Remove(client.ID);

                clients.Add(client.ID, client);
            }

            Message msg = new Message();
            msg.Header = new MessageHeader();
            msg.Header.ClientID = client.ID;

            SendMessageTo(msg, client.ClientIP);
        }

        protected virtual bool SendMessageToAsync(Message message, IPEndPoint remoteIP)
        {
            if (message != null && remoteIP != null)
            {
                var messageByte = Helper.SerializeMessage(message);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.RemoteEndPoint = remoteIP;
                args.SocketFlags = SocketFlags.None;
                args.SetBuffer(messageByte, 0, messageByte.Length);

                return socket.SendToAsync(args);
            }

            return true;
        }

        protected virtual void SendMessageTo(Message message, IPEndPoint remoteIP)
        {
            if (message != null && remoteIP != null)
            {
                var messageByte = Helper.SerializeMessage(message);

                socket.SendTo(messageByte, SocketFlags.None, remoteIP);
            }
        }

        protected virtual void DisconnectClient(string id)
        {
            if (clients.ContainsKey(id))
            {
                var client = clients[id];

                if (client != null)
                {
                    Message msg = new Message();
                    msg.Body = new MessageBody();
                    msg.Body.Message = "OK";

                    var echo = Helper.SerializeMessage(msg);

                    socket.SendTo(echo, System.Net.Sockets.SocketFlags.None, client.ClientIP);

                    lock (lockObj)
                    {
                        if (clients.ContainsKey(id))
                            clients.Remove(id);
                    }
                }
            }
        }

        public void Start()
        {
            StartListening();
            StartStreaming();

            Console.WriteLine("Controller Started");
        }

        private void StartStreaming()
        {
            streamingThread = new Thread(Stream);
            streamingThread.Start();
        }

        private void Stream()
        {
            //while (true)
            //{

            //}
        }

        public void SendImageFrameToClients(byte[] image)
        {
            if (clients != null && clients.Count > 0)
            {
                streamContext.Push(new StreamBufferItem(1, image));

                foreach (var client in clients)
                {
                    var message = new Message();
                    message.Header = new MessageHeader();
                    message.Header.IP = socket.LocalEndPoint;
                    message.Header.PacketID = 0;
                    message.Body = new MessageBody();
                    message.Body.Message = "new image";
                    message.Body.ID = 1;
                    message.Body.Frame = image;

                    SendMessageToAsync(message, client.Value.ClientIP);
                }
            }
        }

        private void Listen()
        {
            while (true)
            {
                var ackArguments = new System.Net.Sockets.SocketAsyncEventArgs();

                ackArguments.Completed += ackArguments_Completed;

                ackArguments.SetBuffer(ackRecieveBuffer, 0, ackRecieveBuffer.Length);

                ackArguments.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                listenFlag.Reset();

                if (!socket.RecieveFromAsync(ackArguments))
                {
                    listenFlag.Set();
                    ackArguments_Completed(socket, ackArguments);
                }

                listenFlag.WaitOne();
            }
        }

        private void StartListening()
        {
            listenigThread = new Thread(Listen);
            listenigThread.Start();
        }

        void ackArguments_Completed(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            listenFlag.Set();

            ProcessIncomingMessage(sender, e);
        }

        private void LoadOptions()
        {
            options = new Connection.ConnectionOptions();
            options.Broadcast = Convert.ToBoolean(ConfigurationManager.AppSettings["ConnectionOptions.Broadcast"]);
            options.InterfaceType = System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211;
            options.Listen = Convert.ToBoolean(ConfigurationManager.AppSettings["ConnectionOptions.Listen"]);
            options.ListeningPortNumber = Convert.ToInt32(ConfigurationManager.AppSettings["ConnectionOptions.ListenPortNumber"]);
            options.AckListeningPortNumber = Convert.ToInt32(ConfigurationManager.AppSettings["ConnectionOptions.AckListeningPortNumber"]);
            options.LoopBack = Convert.ToBoolean(ConfigurationManager.AppSettings["ConnectionOptions.LoopBack"]);
            options.RecieveBufferSize = 65536;
            options.SendBufferSize = 65536;
            options.SendTimeout = 500;
        }

        private void ProcessIncomingMessage(object sender, SocketAsyncEventArgs e)
        {
            if (e != null && e.SocketError == System.Net.Sockets.SocketError.Success)
            {
                Console.WriteLine("Transfer Finished.");
                Console.WriteLine("RemoteEndpoint: {0}", e.RemoteEndPoint.ToString());

                string message = System.Text.Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred);

                byte[] messageBytes = new byte[e.BytesTransferred];

                Buffer.BlockCopy(e.Buffer, 0, messageBytes, 0, messageBytes.Length);

                var incomingMessage = Helper.DeserializeMessage(messageBytes);

                if (incomingMessage != null && incomingMessage.Body != null && incomingMessage.Header != null)
                {
                    SetClientIP(incomingMessage);

                    switch (incomingMessage.Body.Message)
                    {
                        case "Request":
                            {
                                var client = new ClientInformation((IPEndPoint)e.RemoteEndPoint);

                                AcceptClient(client);
                                break;
                            }

                        case "Disconnect":
                            {
                                DisconnectClient(incomingMessage.Header.ClientID);
                                break;
                            }

                        case "NACK":
                            {
                                ResendLostPackage(incomingMessage);
                                break;
                            }
                    }
                }
            }
        }

        private void SetClientIP(Message incomingMessage)
        {
            if (incomingMessage.Header != null && incomingMessage.Header.IP == null && !string.IsNullOrEmpty(incomingMessage.Header.ClientID))
            {
                if (clients.ContainsKey(incomingMessage.Header.ClientID))
                {
                    var clientInformation = clients[incomingMessage.Header.ClientID];
                    if (clientInformation != null)
                    {
                        incomingMessage.Header.IP = clientInformation.ClientIP;
                    }
                }
            }
        }

        private void ResendLostPackage(Message message)
        {
            if (message != null)
            {
                if (streamContext != null)
                {
                    var foundFrame = streamContext.Find(message.Body.ID);

                    if (foundFrame != null)
                    {
                        SendMessageToAsync(message, message.Header.IP);
                    }
                }
            }
        }
    }
}
