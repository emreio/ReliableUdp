using ReliableUdp.Transmission.Utilities;
using ReliableUdp.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reliable = ReliableUdp.Common;

namespace ReliableUdp.ClientWinForm
{
    public partial class Form1 : Form
    {
        UdpClient client;
        IPEndPoint remoteEndPoint;
        string clientID;
        byte[] recieveBuffer = new byte[65536];
        Thread frameRecieveThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frameRecieveThread = new Thread(FetchFrames);
            frameRecieveThread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frameRecieveThread.Abort();
            Disconnect();
            MessageBox.Show("Disconnected");
        }

        private void Disconnect()
        {
            Reliable.Message disconnectMsg = new Reliable.Message();
            disconnectMsg.Body = new Reliable.MessageBody();
            disconnectMsg.Body.Message = "Disconnect";
            disconnectMsg.Header = new Reliable.MessageHeader();
            disconnectMsg.Header.ClientID = clientID;

            var bytes = Helper.SerializeMessage(disconnectMsg);

            client.Send(bytes, bytes.Length, remoteEndPoint);

            client.Close();
            if (client.Client != null)
                client.Client.Close();

            clientID = null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string remoteIP = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["RemoteIP"]);
            int remotePort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["RemotePort"]);

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);

            if (string.IsNullOrEmpty(clientID))
            {
                AcquireClientID();
            }
        }

        ManualResetEvent flag = new ManualResetEvent(false);

        private void FetchFrames()
        {
            if (string.IsNullOrEmpty(clientID))
            {
                AcquireClientID();
            }

            while (true)
            {
                flag.Reset();

                using (var asyncArgs = new SocketAsyncEventArgs())
                {
                    asyncArgs.Completed += asyncArgs_Completed;
                    asyncArgs.RemoteEndPoint = remoteEndPoint;
                    asyncArgs.SetBuffer(recieveBuffer, 0, recieveBuffer.Length);
                    if (client.Client.ReceiveAsync(asyncArgs))
                    {
                        asyncArgs_Completed(client, asyncArgs);
                    }
                }

                flag.WaitOne();
            }
        }

        void asyncArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                flag.Set();

                if (e.SocketError == SocketError.Success)
                {
                    byte[] recieved = new byte[e.BytesTransferred];

                    Buffer.BlockCopy(e.Buffer, 0, recieved, 0, recieved.Length);

                    if (recieved.Length > 0)
                    {
                        var frameMessage = Helper.DeserializeMessage(recieved);

                        if (frameMessage != null && frameMessage.Header != null && frameMessage.Body != null)
                        {
                            if (frameMessage.Body.Frame != null)
                            {
                                using (MemoryStream ms = new MemoryStream(frameMessage.Body.Frame))
                                {
                                    var image = Image.FromStream(ms);
                                    pictureBox1.Image = image;
                                }
                            }
                        }
                    }
                }

                else
                {
                    //MessageBox.Show(e.SocketError.ToString());
                    //if (e.ConnectSocket != null)
                    //{
                    //    e.ConnectSocket.Close();
                    //    e.ConnectSocket.Dispose();
                    //}

                    if (client != null)
                    {
                        client.Close();
                        if (client.Client != null)
                        {
                            client.Client.Disconnect(false);
                            client.Client.Dispose();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logger.LogExeption(ex);
            }

            Thread.Sleep(30);
        }

        private void AcquireClientID()
        {
            Reliable.Message requestMessage = new Reliable.Message();
            requestMessage.Body = new Reliable.MessageBody();
            requestMessage.Body.Message = "Request";
            requestMessage.Header = new Reliable.MessageHeader();
            byte[] requestBytes = Helper.SerializeMessage(requestMessage);

            InitializeClient();

            //client.Connect(remoteEndPoint);
            //client.Client.Bind(new IPEndPoint(IPAddress.Loopback, Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LocalPort"])));

            client.Send(requestBytes, requestBytes.Length, remoteEndPoint);

            byte[] responseBytes = client.Receive(ref remoteEndPoint);

            Console.WriteLine(responseBytes.Length);
            var responseMessage = Helper.DeserializeMessage(responseBytes);

            if (responseMessage != null && responseMessage.Header != null)
            {
                clientID = responseMessage.Header.ClientID;

                MessageBox.Show("Connected to the server, ClientID: " + clientID);
            }
        }

        private void InitializeClient()
        {
            int localPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LocalPort"]);

            client = new UdpClient(localPort);
        }
    }
}
