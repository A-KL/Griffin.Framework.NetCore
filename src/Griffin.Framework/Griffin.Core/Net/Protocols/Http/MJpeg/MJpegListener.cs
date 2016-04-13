using System;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.MJpeg
{
    public class MJpegListener : HttpListener
    {        

        /// <summary>
        ///     Create a new instance of <see cref="MJpegListener" />.
        /// </summary>
        /// <param name="configuration">Custom server configuration</param>
        public MJpegListener(ChannelTcpListenerConfiguration configuration)
            : base(configuration)
        {
        }

        /// <summary>
        ///     Create a new instance of  <see cref="MJpegListener" />.
        /// </summary>
        public MJpegListener()
        {
            var config = new ChannelTcpListenerConfiguration(
                () => new HttpMessageDecoder(),
                () => new MJpegEncoder());

            Configure(config);
        }


        /// <summary>
        /// Handles the upgrade
        /// </summary>
        /// <param name="source">Channel that we've received a request from</param>
        /// <param name="msg">Message received.</param>
        protected override void OnMessage(ITcpChannel source, object msg)
        {
            var request = msg as IHttpRequest;
            if (request != null)
            {
                var args = new LiveStreamRequestEventArgs(source, request);

                this.LiveStreamRequest(this, args);

                //args.ContentReadFunc()

                return;
            }

            base.OnMessage(source, msg);
        }

        public EventHandler<LiveStreamRequestEventArgs> LiveStreamRequest = delegate { };
    }
}
