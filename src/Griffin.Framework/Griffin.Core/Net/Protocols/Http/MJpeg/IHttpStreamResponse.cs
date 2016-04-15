using Griffin.Net.Protocols.Http;

namespace Griffin.Core.Net.Protocols.Http.MJpeg
{
    public interface IHttpStreamResponse : IHttpResponse
    {
        IFramesSource StreamSource { get; }
    }
}