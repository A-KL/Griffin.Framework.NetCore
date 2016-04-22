namespace Griffin.Core.Net.Protocols.Http.MJpeg
{
    public sealed class LiveStreamMessage
    {
        public IFramesSource Source { get; }

        public LiveStreamMessage(IFramesSource source)
        {
            this.Source = source;
        }
    }
}
