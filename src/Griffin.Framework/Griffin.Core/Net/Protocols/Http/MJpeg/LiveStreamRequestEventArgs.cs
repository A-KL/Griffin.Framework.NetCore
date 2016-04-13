using System;
using System.IO;
using System.Threading.Tasks;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.MJpeg
{
    public class LiveStreamRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new isntance of <see cref="LiveStreamRequestEventArgs"/>
        /// </summary>
        /// <param name="channel">Channel used for transfers</param>
        /// <param name="request">Request (should contain the upgrade request)</param>
        public LiveStreamRequestEventArgs(ITcpChannel channel, IHttpRequest request)
        {
            this.Channel = channel;
            this.Request = request;
        }

        /// <summary>
        /// Channel for the connected client
        /// </summary>
        public ITcpChannel Channel { get; private set; }

        /// <summary>
        /// WebSocket handshake request
        /// </summary>
        public IHttpRequest Request { get; private set; }


        public Func<Stream, IHttpRequest, IHttpResponse, Task> ContentReadFunc;
    }
}
