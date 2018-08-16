using ComicViewer.ComicViewModel;
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
    public class ComicFile : IDocument
    {
        IArchive _archive;
        uint _totalPages;
        public ComicFile(IRandomAccessStream documentStream)
        {
            _archive = ArchiveFactory.Open(documentStream.AsStreamForRead());
        }

        public void WriteTo(ComicImageViewModelList listWriteTo)
        {
            _totalPages = 1;
            //iterate and create number of files and create appropriate model objects.
            var imageList = _archive.Entries.ToList();
            imageList.Sort((cmp1, cmp2) => cmp1.Key.CompareTo(cmp2.Key));
            foreach (var item in imageList)
            {
                var imgType = item.GetImageType();
                if (imgType != ImageType.None)
                {
                    listWriteTo.Add(new ComicImageViewModel(new ComicImage(new ComicPage(item, _totalPages), imgType), _totalPages));
                    _totalPages++;
                }
            }
        }


        public uint TotalPages { get => _totalPages; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_archive != null)
                    {
                        _archive.Dispose();
                        _archive = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ComicFile() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
}
