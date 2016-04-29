namespace Griffin.Core.Net.Protocols.Http.Multipart
{
    using System;
    using System.Threading.Tasks;

    public interface IFramesSource : IDisposable
    {
        Task<bool> WriteNextFrame(MultipartStream stream);
    }
}
