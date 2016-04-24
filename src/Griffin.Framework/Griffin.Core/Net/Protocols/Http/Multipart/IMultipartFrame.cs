using System;
using Windows.Storage.Streams;

namespace Griffin.Core.Net.Protocols.Http.Multipart
{
    public interface IMultipartFrame : IDisposable
    {
        IBuffer Data { get; }

        int DataSize { get; }
    }
}
