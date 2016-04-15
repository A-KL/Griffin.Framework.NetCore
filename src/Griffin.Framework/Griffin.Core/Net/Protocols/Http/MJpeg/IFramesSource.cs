using System.Collections.Generic;

namespace Griffin.Core.Net.Protocols.Http.MJpeg
{
    public interface IFramesSource
    {
        IEnumerable<IImageFrame> Frames { get; }
    }
}
