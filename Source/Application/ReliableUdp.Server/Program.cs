using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliableUdp.Transmission;
using ReliableUdp.ImageAcquisition;
using System.IO;
namespace ReliableUdp.Server
{
    class Program
    {
        static Controller controller = null;

        static void Main(string[] args)
        {
            controller = new Controller();

            controller.Start();

            Console.WriteLine("Transmission controller started.");

            DeviceVideoCaptureManager captureManager = new DeviceVideoCaptureManager();

            captureManager.NewFrame += captureManager_NewFrame;

            var camera = captureManager.ListAllVideoDevices().First();

            captureManager.StartCapturing(camera, captureManager_NewFrame);

            //while (true)
            //{
            //    System.Threading.Thread.Sleep(5000);

            //    byte[] buffer = new byte[200];

            //    new Random().NextBytes(buffer);

            //    controller.SendImageFrameToClients(buffer);
            //}

            Console.ReadLine();
        }

        static void captureManager_NewFrame(object sender, global::ImageAcquisition.CaptureNewFrameEventArgs args)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                args.Frame.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                if (controller != null)
                {
                    controller.SendImageFrameToClients(ms.ToArray());
                }
            }
        }
    }
}
