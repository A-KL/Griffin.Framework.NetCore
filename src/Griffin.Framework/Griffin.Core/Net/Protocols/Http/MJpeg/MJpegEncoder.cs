using System;
using System.IO;
using Griffin.Core.Net.Protocols.Http.MJpeg;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.MJpeg
{
    public class MJpegEncoder : IMessageEncoder
    {
        private readonly byte[] _buffer = new byte[65535];

        private readonly MemoryStream _stream;
        private readonly StreamWriter _writer;

        private int _bytesToSend;
        private int _totalAmountToSend;
        private int _offset;

        private bool _isHeaderSent;

        private object _resetLock = new object();

        private IHttpStreamResponse _message;

        public void Prepare(object message)
        {
            var liveSteram = message as IHttpStreamResponse;
            if (liveSteram == null)
            {
                throw new InvalidOperationException("This encoder only supports messages deriving from 'IHttpStreamResponse'");
            }

            _message = liveSteram;

            if (_message.Body != null && _message.Body.Length != 0)
            {
                _message.Body.Dispose();
                _message.Body = null;
            }

            _message.Headers["Content-Length"] = null;

        }

        public void Send(ISocketBuffer buffer)
        {
            // last send operation did not send all bytes enqueued in the buffer
            // so let's just continue until doing next message
            if (_bytesToSend > 0)
            {
                buffer.SetBuffer(_buffer, _offset, _bytesToSend);
                return;
            }

            if (!_isHeaderSent)
            {
                _writer.WriteLine(_message.StatusLine);

                foreach (var header in _message.Headers)
                {
                    _writer.Write("{0}: {1}\r\n", header.Key, header.Value);
                }

                _writer.Write("\r\n");
                _writer.Flush();
                _isHeaderSent = true;
                buffer.UserToken = _message;
            }


                // continuing with the message body
            if (_isHeaderSent)
            {
                var bytes = Math.Min(_totalAmountToSend, _buffer.Length);
                _message.Body.Read(_buffer, 0, bytes);
                _bytesToSend = bytes;

                buffer.SetBuffer(_buffer, 0, bytes);
                return;
            }


            if (_message.Body == null || _message.ContentLength == 0)
            {
                _bytesToSend = (int)_stream.Length;
                _totalAmountToSend = _bytesToSend;
                buffer.SetBuffer(_buffer, 0, (int)_stream.Length);
                return;
            }

            //var bytesLeft = _buffer.Length - _stream.Length;

            //var bytesToSend = Math.Min(_message.ContentLength, (int)bytesLeft);
            //var offset = (int)_stream.Position;
            //_message.Body.Read(_buffer, offset, bytesToSend);
            //_bytesToSend = (int)_stream.Length + bytesToSend;
            //_totalAmountToSend = (int)_stream.Length + _message.ContentLength;
            //buffer.SetBuffer(_buffer, 0, _bytesToSend);



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
            _totalAmountToSend -= bytesTransferred;
            _bytesToSend -= bytesTransferred;
            _offset += bytesTransferred;

            if (_bytesToSend <= 0)
                _offset = 0;

            if (_totalAmountToSend == 0)
            {
                Clear();
            }

            return _totalAmountToSend <= 0;
        }

        public void Clear()
        {
            if (_message != null)
            {
                lock (_resetLock)
                {
                    if (_message != null && _message.Body != null)
                        _message.Body.Dispose();

                    _message = null;
                }
            }

            _bytesToSend = 0;
            _isHeaderSent = false;
            _stream.SetLength(0);
        }
    }
}
