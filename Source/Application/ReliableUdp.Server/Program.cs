using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliableUdp.Transmission;

namespace ReliableUdp.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new Controller();
            controller.Start();

            while (true)
            {
                System.Threading.Thread.Sleep(5000);
                
                byte[] buffer = new byte[200];

                new Random().NextBytes(buffer);

                controller.SendImageFrameToClients(buffer);
            }
        }
    }
}
