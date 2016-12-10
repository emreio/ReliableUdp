using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAcquisition
{
    public class CaptureNewFrameEventArgs : EventArgs
    {
        private Bitmap frame;

        public CaptureNewFrameEventArgs(Bitmap image)
        {
            this.frame = image;
        }

        public Bitmap Frame
        {
            get
            {
                return frame;
            }
        }
    }
}
