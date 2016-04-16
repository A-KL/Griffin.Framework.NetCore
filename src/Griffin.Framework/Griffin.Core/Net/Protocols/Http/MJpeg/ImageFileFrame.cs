using System;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Griffin.Core.Net.Protocols.Http.MJpeg
{
    public class ImageFileFrame : IImageFrame
    {
        public ImageFileFrame(IStorageFile file)
        {
            this.Data = FileIO.ReadBufferAsync(file).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            
        }

        public int Height { get; }

        public int Width { get; }

        public IBuffer Data { get; }
    }
}
