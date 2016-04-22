using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Griffin.Core.Net.Protocols.Http.MJpeg;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.MJpeg
{
    public class MJpegEncoder : IMessageEncoder
    {
        private readonly MemoryStream stream;
        private readonly StreamWriter writer;

        private int bytesToSend;

        private bool isHeaderSent;

        private IHttpStreamResponse message;

        private IEnumerator<IImageFrame> framesEnumerator;

        private string boundary { get; } = "boundary";

        /// <summary>
        /// Initializes a new instance of the <see cref="MJpegEncoder"/> class.
        /// </summary>
        public MJpegEncoder()
        {
            this.stream = new MemoryStream(65535);
           this.stream.SetLength(0);
            this.writer = new StreamWriter(this.stream);
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
            this.message.Headers["Content-Type"] = "multipart/x-mixed-replace;boundary=" + boundary;

            this.framesEnumerator = this.message.StreamSource.Frames.GetEnumerator();
            this.framesEnumerator.MoveNext();
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
            }

            // Write frame
            var frame = this.framesEnumerator.Current;

            // Frame header
            var frameHeader = $"--{this.boundary}\r\nContent-Type: image/jpeg\r\nContent-Length: {frame.Data.Length}\r\n\r\n";
            this.writer.Write(frameHeader);

            this.writer.Flush();

            // Frame body
            this.stream.Write(frame.Data.ToArray(), 0, (int)frame.Data.Length);

            // Send
            var streamBuffer = this.stream.ToArray();

            buffer.SetBuffer(streamBuffer, 0, streamBuffer.Length);
        }

        public bool OnSendCompleted(int bytesTransferred)
        {
            return !this.framesEnumerator.MoveNext();
        }

        public void Clear()
        {
            bytesToSend = 0;
            isHeaderSent = false;
            stream.SetLength(0);
        }
    }
}
