using System;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.MJpeg
{
    public class MJpegEncoder : IMessageEncoder
    {
        public void Prepare(object message)
        {
            throw new NotImplementedException();
        }

        public void Send(ISocketBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public bool OnSendCompleted(int bytesTransferred)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
