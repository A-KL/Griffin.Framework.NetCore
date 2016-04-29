using System.Diagnostics;

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

        private IHttpStreamResponse response;
        
       // private Stopwatch stopwatch = new Stopwatch();

        public MultipartEncoder()
        {
            this.stream = new MemoryStream(this.buffer);
            this.stream.SetLength(0);

            this.writer = new StreamWriter(this.stream);
            this.multipartStream = new MultipartStream(this.stream);
        }

        public void Prepare(object message)
        {
            var streamResponse = message as IHttpStreamResponse;
            if (streamResponse == null)
            {
                throw new InvalidOperationException("This encoder only supports messages deriving from 'IHttpStreamResponse'");
            }

            this.response = streamResponse;

            if (this.response.Body != null && this.response.Body.Length != 0)
            {
                this.response.Body.Dispose();
                this.response.Body = null;
            }

            this.response.Headers["Content-Length"] = string.Empty;
            this.response.Headers["Content-Type"] = "multipart/x-mixed-replace;boundary=" + this.multipartStream.Boundary;
        }

        public void Send(ISocketBuffer buffer)
        {
            this.stream.Position = 0;
            this.stream.SetLength(0);

            if (!this.isHeaderSent)
            {
                this.writer.WriteLine(this.response.StatusLine);

                foreach (var header in this.response.Headers)
                {
                    if (string.IsNullOrEmpty(header.Key))
                    {
                        continue;
                    }

                    this.writer.Write("{0}: {1}\r\n", header.Key, header.Value);
                }

                this.writer.Write("\r\n");


                this.isHeaderSent = true;
                buffer.UserToken = this.response;
                this.writer.Flush();
            }

            //stopwatch.Stop();

            //Debug.WriteLine("Network: " + stopwatch.ElapsedMilliseconds);

            //stopwatch.Restart();

            this.nextFrameAvailable = this.response.StreamSource.WriteNextFrame(this.multipartStream).Result;

            //stopwatch.Stop();

            //Debug.WriteLine("Encoding: " + stopwatch.ElapsedMilliseconds);

            //stopwatch.Restart();

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
