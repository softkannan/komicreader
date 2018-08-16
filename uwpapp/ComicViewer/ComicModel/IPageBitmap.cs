using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ComicViewer.ComicModel
{
    public interface IPageBitmap
    {
        void RenderImage(InMemoryRandomAccessStream streamToWriteTo);
        void RenderImage(InMemoryRandomAccessStream streamToWriteTo,uint width,uint height);
    }
}
