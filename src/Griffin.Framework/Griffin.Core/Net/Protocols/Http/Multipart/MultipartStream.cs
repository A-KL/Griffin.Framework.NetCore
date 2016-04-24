namespace Griffin.Core.Net.Protocols.Http.Multipart
{
    using System.IO;
    using System.Text;

    public class MultipartStream : Stream
    {
        private static readonly string FrameHeaderFormat = "--{0}\r\nContent-Type: image/jpeg\r\nContent-Length: {1}\r\n\r\n";

        private readonly Stream innerStream;

        public MultipartStream(Stream innerStream, string boundary = "boundary")
        {
            this.innerStream = innerStream;
            this.Boundary = boundary;
        }

        public string Boundary { get; }

        public override bool CanRead => this.innerStream.CanRead;

        public override bool CanSeek => this.innerStream.CanRead;

        public override bool CanWrite => this.innerStream.CanWrite;

        public override long Length => this.innerStream.Length;

        public override long Position
        {
            get { return this.innerStream.Position; }
            set { this.innerStream.Position = value; }
        }


        public override void Flush()
        {
            this.innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.innerStream.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var binHeader = Encoding.ASCII.GetBytes(string.Format(FrameHeaderFormat, this.Boundary, count));

            this.innerStream.Write(binHeader, 0, binHeader.Length);
            this.innerStream.Write(buffer, offset, count);
        }
    }
}
