using Griffin.Net.Protocols.Http;

namespace Griffin.Core.Net.Protocols.Http.Multipart
{
    public interface IHttpStreamResponse : IHttpResponse
    {
        IFramesSource StreamSource { get; }
    }
}