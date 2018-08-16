using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ComicViewer.ComicModel
{
    public class ComicPage : IPageBitmap
    {
        private IArchiveEntry _archivePage;
        private uint _pageNo;
        public ComicPage(IArchiveEntry archivePage, uint pageNo)
        {
            _archivePage = archivePage;
            _pageNo = pageNo;
        }

        public void RenderImage(InMemoryRandomAccessStream streamToWriteTo, uint width, uint height)
        {
            _archivePage.WriteTo(streamToWriteTo.AsStream());
        }

        public void RenderImage(InMemoryRandomAccessStream streamToWriteTo)
        {
            _archivePage.WriteTo(streamToWriteTo.AsStream());
        }
    }
}
