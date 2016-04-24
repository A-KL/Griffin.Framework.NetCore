using System;
using Griffin.Core.Net.Protocols.Http.MJpeg;

namespace Griffin.Net.Protocols.Http.MJpeg
{
    public class FrameReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new isntance of <see cref="FrameReceivedEventArgs"/>
        /// </summary>
        /// <param name="frame">Channel used for transfers</param>        
        public FrameReceivedEventArgs(IImageFrame frame)
        {
            this.Frame = frame;
            this.IsLastFrame = false;
        }

        public IImageFrame Frame { get; set; }

        public bool IsLastFrame { get; set; }
    }
}
