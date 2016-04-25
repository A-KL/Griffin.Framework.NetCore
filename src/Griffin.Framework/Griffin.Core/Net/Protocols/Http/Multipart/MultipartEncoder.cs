namespace Griffin.Core.Net.Protocols.Http.Multipart
{
    using System;
    using System.IO;
    using Griffin.Net;
    using Griffin.Net.Channels;

    public class MultipartEncoder : IMessageEncoder
    {
        private readonly MultipartStream multipartStream;
        private readonly MemoryStream stream;
        private readonly StreamWriter writer;

        private readonly byte[] buffer = new byte[65535];

        private bool nextFrameAvailable;
        private bool isHeaderSent;

        private IHttpStreamResponse message;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartEncoder"/> class.
        /// </summary>
        public MultipartEncoder()
        {
            this.stream = new MemoryStream(this.buffer);
            this.stream.SetLength(0);

            this.writer = new StreamWriter(this.stream);
            this.multipartStream = new MultipartStream(this.stream);
        }

        public void Prepare(object message)
        {
            var liveSteram = message as IHttpStreamResponse;
            if (liveSteram == null)
            {
                throw new InvalidOperationException("This encoder only supports messages deriving from 'IHttpStreamResponse'");
            }

            this.message = liveSteram;

            if (this.message.Body != null && this.message.Body.Length != 0)
            {
                this.message.Body.Dispose();
                this.message.Body = null;
            }

            this.message.Headers["Content-Length"] = string.Empty;
            this.message.Headers["Content-Type"] = "multipart/x-mixed-replace;boundary=" + this.multipartStream.Boundary;
        }

        public void Send(ISocketBuffer buffer)
        {
            this.stream.Position = 0;
            this.stream.SetLength(0);

            if (!this.isHeaderSent)
            {
                this.writer.WriteLine(this.message.StatusLine);

                foreach (var header in this.message.Headers)
                {
                    if (string.IsNullOrEmpty(header.Key))
                    {
                        continue;
                    }

                    this.writer.Write("{0}: {1}\r\n", header.Key, header.Value);
                }

                this.writer.Write("\r\n");


                this.isHeaderSent = true;
                buffer.UserToken = this.message;
                this.writer.Flush();
            }

            // Write frame
            //var frame = this.framesEnumerator.Current;

            // Frame header
            //this.multipartStream.Write();

            //var frameHeader = $"--{this.boundary}\r\nContent-Type: image/jpeg\r\nContent-Length: {frame.DataSize}\r\n\r\n";
            //this.writer.Write(frameHeader);

            // Frame body & header
            // this.multipartStream.Write(frame.Data.ToArray(), 0, (int)frame.DataSize);

            this.nextFrameAvailable = this.message.StreamSource.WriteNextFrame(this.multipartStream);

            // Send
            buffer.SetBuffer(this.buffer, 0, (int)this.stream.Length);
        }

        public bool OnSendCompleted(int bytesTransferred)
        {
            return !this.nextFrameAvailable;
        }

        public void Clear()
        {            
            this.isHeaderSent = false;
            this.stream.SetLength(0);
        }
    }
}
