using ReliableUdp.Common;
using ReliableUdp.Transmission;
using ReliableUdp.Transmission.Utilities;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ReliableUdp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            new Thread(x =>
            {
                var mainController = new Controller();
                mainController.Start();
            }).Start();


            Message message = new Message();
            message.Body = new MessageBody();
            message.Body.Message = "Request";

            var client = new UdpClient(5656);

            client.Connect(new IPEndPoint(IPAddress.Loopback, 9595));

            //client.Client.Bind(new IPEndPoint(IPAddress.Loopback, 5656));

            var outgoingBytes = Helper.SerializeMessage(message);

            client.Send(outgoingBytes, outgoingBytes.Length);

            IPEndPoint remoteIP = new IPEndPoint(IPAddress.Loopback, 9595);

            byte[] recieved = client.Receive(ref remoteIP);

            Message msg = Helper.DeserializeMessage(recieved);

            System.Console.WriteLine(msg.Header.ClientID);

            message.Header = new MessageHeader();
            message.Header.ClientID = msg.Header.ClientID;

            message.Body.Message = "Disconnect";

            var outgoingBytes2 = Helper.SerializeMessage(message);

            client.Send(outgoingBytes2, outgoingBytes2.Length);

            byte[] recieved2 = client.Receive(ref remoteIP);

            Message msg2 = Helper.DeserializeMessage(recieved2);

            System.Console.WriteLine(msg.Body.Message);
        }
    }
}
