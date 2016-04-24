using System.Net;
using Griffin.Net.Protocols.Http;

namespace Griffin.Core.Net.Protocols.Http.Multipart
{
    public class HttpStreamResponse : HttpResponse, IHttpStreamResponse
    {
        public HttpStreamResponse(int statusCode, string reasonPhrase, string httpVersion) 
            : base(statusCode, reasonPhrase, httpVersion)
        {
        }

        public HttpStreamResponse(HttpStatusCode statusCode, string reasonPhrase, string httpVersion) 
            : base(statusCode, reasonPhrase, httpVersion)
        {
        }

        public IFramesSource StreamSource { get; set; }
    }
}
