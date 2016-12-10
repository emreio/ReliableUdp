using ReliableUdp.Common;
using ReliableUdp.Transmission.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Client
{
    class Program
    {
        static string clientID = null;
        static UdpClient client;
        static IPEndPoint remote = new IPEndPoint(IPAddress.Loopback, 9595);
        static int count = 0;

        static void Main(string[] args)
        {
            client = new UdpClient(5656);

            if (clientID == null)
            {
                AcquireClientID();
            }

            while (true)
            {
                byte[] image = client.Receive(ref remote);
                Message m = Helper.DeserializeMessage(image);
                if (m != null)
                {
                    string msg = System.Text.Encoding.ASCII.GetString(m.Body.Frame);
                    Console.WriteLine(msg);
                    count++;

                    if (count > 5)
                        break;
                }
            }

            Message disconnectMsg = new Message();
            disconnectMsg.Header = new MessageHeader();
            disconnectMsg.Header.ClientID = clientID;
            disconnectMsg.Body = new MessageBody();
            disconnectMsg.Body.Message = "Disconnect";

            var disconnectBytes = Helper.SerializeMessage(disconnectMsg);

            client.Send(disconnectBytes, disconnectBytes.Length, remote);

            var disconnectRes = client.Receive(ref remote);

            var dm = Helper.DeserializeMessage(disconnectRes);

            if (dm != null)
            {
                Console.WriteLine(dm.Body.Message);
            }

            Console.WriteLine("Finished");

            Console.ReadLine();
        }

        private static void AcquireClientID()
        {
            Message requestMsg = new Message();
            requestMsg.Body = new MessageBody();
            requestMsg.Header = new MessageHeader();
            requestMsg.Body.Message = "Request";

            var message = Helper.SerializeMessage(requestMsg);

            client.Send(message, message.Length, remote);

            var response = client.Receive(ref remote);

            var responseMessage = Helper.DeserializeMessage(response);

            if (responseMessage != null && responseMessage.Body != null && responseMessage.Header != null)
            {
                clientID = responseMessage.Header.ClientID;
            }
        }
    }
}
