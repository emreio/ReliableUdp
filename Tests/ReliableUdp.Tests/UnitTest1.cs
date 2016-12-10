using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReliableUdp.Connection;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace ReliableUdp.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            new Server();
        }


        class Server
        {
            IUdpConnection connectionObject;

            ManualResetEvent flag = new ManualResetEvent(false);

            public Server()
            {
                ConnectionOptions options = new ConnectionOptions();
                options.Broadcast = true;
                options.InterfaceType = System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211;
                options.Listen = true;
                options.ListeningPortNumber = 9999;
                options.LoopBack = false;
                options.RecieveBufferSize = 500;
                options.RecieveTimeout = 10000;
                options.SendBufferSize = 500;
                options.SendTimeout = 10000;

                BaseConnection connection = new Connection.Connection(options);

                byte[] buffer = System.Text.Encoding.ASCII.GetBytes("hello");
                byte[] buffer2 = System.Text.Encoding.ASCII.GetBytes("hello");

                connection.SendTo(buffer, SocketFlags.None, new IPEndPoint(IPAddress.Broadcast, 6565));

                var socketArg = new SocketAsyncEventArgs();

                socketArg.SetBuffer(buffer, 0, buffer.Length);
                socketArg.SocketFlags = SocketFlags.None;
                socketArg.UserToken = connection;
                socketArg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                socketArg.Completed += socketArg_Completed;
                socketArg.AcceptSocket = connection.socket;

                while (true)
                {
                    flag.Reset();

                    connection.RecieveFromAsync(socketArg);

                    flag.WaitOne();
                }

            }

            void socketArg_Completed(object sender, SocketAsyncEventArgs e)
            {
                var server = e.UserToken as IUdpConnection;

                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Accept:
                        break;
                    case SocketAsyncOperation.Connect:
                        break;
                    case SocketAsyncOperation.Disconnect:
                        break;
                    case SocketAsyncOperation.None:
                        break;
                    case SocketAsyncOperation.Receive:
                        break;
                    case SocketAsyncOperation.ReceiveFrom:
                        {
                            Console.WriteLine("{0} bytes recieved from {1}", e.BytesTransferred, e.RemoteEndPoint.ToString());

                            string message = System.Text.Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred);

                            SocketAsyncEventArgs arg2 = new SocketAsyncEventArgs();
                            
                            arg2.SetBuffer(System.Text.Encoding.ASCII.GetBytes("echo"), 0, 4);
                            arg2.RemoteEndPoint = e.RemoteEndPoint;
                            arg2.Completed += socketArg_Completed;
                            arg2.SocketFlags = SocketFlags.None;

                            server.SendToAsync(arg2);

                            flag.Set();
                        }
                        break;
                    case SocketAsyncOperation.ReceiveMessageFrom:
                        break;
                    case SocketAsyncOperation.Send:
                        {
                            Console.WriteLine("Sent.");
                        }
                        break;
                    case SocketAsyncOperation.SendPackets:
                        break;
                    case SocketAsyncOperation.SendTo:
                        break;
                    default:
                        break;
                }

                Console.WriteLine(e.BytesTransferred);
            }
        }
    }
}
