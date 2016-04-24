namespace Griffin.Core.Net.Protocols.Http.Multipart
{
    public interface IFramesSource
    {
        bool WriteNextFrame(MultipartStream stream);
    }
}
