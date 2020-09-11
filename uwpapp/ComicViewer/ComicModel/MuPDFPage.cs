using ComicViewer.ComicViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ComicViewer.ComicModel
{
    public class MuPDFPage : IPageBitmap
    {
        public void RenderImage(InMemoryRandomAccessStream streamToWriteTo)
        {
            throw new NotImplementedException();
        }

        public void RenderImage(InMemoryRandomAccessStream streamToWriteTo, uint width, uint height)
        {
            throw new NotImplementedException();
        }
    }
}
