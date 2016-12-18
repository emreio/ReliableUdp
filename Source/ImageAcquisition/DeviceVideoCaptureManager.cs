using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using AForge.Video;
using ImageAcquisition;

namespace ReliableUdp.ImageAcquisition
{
    public class DeviceVideoCaptureManager
    {
        private readonly FilterInfoCollection filterCollection = null;
        private VideoCaptureDevice videoCaptureDevice = null;
        private bool init = false;
        private bool isStarted = false;
        private bool isCapturing = false;
        private bool async = false;

        public delegate void CapturedNewFrameEventHandler(object sender, CaptureNewFrameEventArgs args);
        private CapturedNewFrameEventHandler newFrameHandler;

        public event CapturedNewFrameEventHandler NewFrame
        {
            add
            {
                //if (!newFrameHandler.GetInvocationList().Contains(value))
                newFrameHandler += value;
            }

            remove
            {
                newFrameHandler -= value;
            }
        }

        public DeviceVideoCaptureManager()
        {
            try
            {
                filterCollection = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);

                init = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }


        //TODO
        public IEnumerable<string> ListAllVideoDevices()
        {
            if (init)
            {
                foreach (FilterInfo item in filterCollection)
                {
                    yield return item.Name;
                }
            }
        }

        public void StartCapturing(string deviceName, CapturedNewFrameEventHandler handler)
        {
            if (!isCapturing)
            {
                if (string.IsNullOrEmpty(deviceName)) throw new ArgumentNullException("Please provide Device Name");

                string monikerName = string.Empty;

                foreach (FilterInfo item in filterCollection)
                {
                    if (item.Name == deviceName)
                    {
                        monikerName = item.MonikerString;
                        break;
                    }
                }

                videoCaptureDevice = new VideoCaptureDevice(monikerName);

                videoCaptureDevice.NewFrame += videoCaptureDevice_NewFrame;

                videoCaptureDevice.Start();

                isCapturing = true;
            }

            else
                throw new ApplicationException("Capture is already running");
        }

        void videoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (newFrameHandler != null && newFrameHandler.GetInvocationList().Count() > 0)
            {
                CaptureNewFrameEventArgs arguments = new CaptureNewFrameEventArgs(eventArgs.Frame);

                newFrameHandler(sender, arguments);
            }
        }

        public bool StartCapturingAsync()
        {
            throw new NotImplementedException();
        }

        public void StartCapturing(string e)
        {

        }
    }
}
