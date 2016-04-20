using System;
using Griffin.Net.Protocols.Http.MJpeg;

namespace Griffin.Core.Net.Protocols.Http.MJpeg
{
    public interface IFramesSource
    {
        event EventHandler<FrameReceivedEventArgs> FrameReceived;
    }
}
