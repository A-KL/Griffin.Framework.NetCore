namespace Griffin.Core.Net.Protocols.Http.Multipart
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IFramesSource : IDisposable
    {
        Task<bool> WriteNextFrame(Stream stream);
    }
}
