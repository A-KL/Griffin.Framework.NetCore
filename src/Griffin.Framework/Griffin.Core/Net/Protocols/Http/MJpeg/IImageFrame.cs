using System;
using Windows.Storage.Streams;

namespace Griffin.Core.Net.Protocols.Http.MJpeg
{
    public interface IImageFrame : IDisposable
    {
        int Height { get; }

        int Width { get; }

        IBuffer Data { get; }
    }
}
