using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Griffin.Core.Net.Protocols.Http.MJpeg;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.MJpeg
{
    public class MJpegEncoder : IMessageEncoder
    {
        private readonly MemoryStream stream;
        private readonly StreamWriter writer;

        private int bytesToSend;
        private int totalAmountToSend;
        private int offset;

        private bool isHeaderSent;

        //private object _resetLock = new object();

        private IHttpStreamResponse message;

        private IEnumerator<IImageFrame> framesEnumerator;

        private string boundary { get; } = "boundary";

        /// <summary>
        /// Initializes a new instance of the <see cref="MJpegEncoder"/> class.
        /// </summary>
        public MJpegEncoder()
        {
            this.stream = new MemoryStream(65535);
           // this.stream.SetLength(0);
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
            // last send operation did not send all bytes enqueued in the buffer
            // so let's just continue until doing next message
            //if (bytesToSend > 0)
            //{
            //    buffer.SetBuffer(_buffer, offset, _bytesToSend);
            //    return;
            //}

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

            //if (this.stream.TryGetBuffer(out streamBuffer))
            //{
                buffer.SetBuffer(streamBuffer, 0, streamBuffer.Length);
            //}

            Task.Delay(1000).Wait();

            //using (var stream = new MemoryStream())
            //{
            //    stream.SetLength(0);

            //    using (var writer = new StreamWriter(stream))
            //    {
            //        foreach (var imageFrame in this.source.Frames)
            //        {
            //            //writer.WriteLine();



            //            writer.Flush();

            //            this.buffer = stream.ToArray();
            //            this.bytesToSend = this.buffer.Length;

            //            buffer.SetBuffer(this.buffer, 0, this.bytesToSend);
            //        }
            //    }
            //}
        }

        public bool OnSendCompleted(int bytesTransferred)
        {
            return !this.framesEnumerator.MoveNext();
        }

        public void Clear()
        {
            //if (message != null)
            //{
            //    lock (resetLock)
            //    {
            //        if (message != null && message.Body != null)
            //            message.Body.Dispose();

            //        message = null;
            //    }
            //}

            bytesToSend = 0;
            isHeaderSent = false;
            stream.SetLength(0);
        }
    }
}
