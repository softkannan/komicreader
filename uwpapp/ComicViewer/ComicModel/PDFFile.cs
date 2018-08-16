using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComicViewer.ComicViewModel;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ComicViewer.ComicModel
{
    public class PDFFile : IDocument
    {
        PdfDocument _pdfDcoument;
        uint _totalPages;
        public PDFFile(PdfDocument pdfDocument)
        {
            _pdfDcoument = pdfDocument;
        }

        public uint TotalPages { get => _totalPages; }

        public void WriteTo(ComicImageViewModelList listWriteTo)
        {
            _totalPages = _pdfDcoument.PageCount;
            //iterate and create number of files and create appropriate model objects.
            for(uint index = 0; index < _totalPages;index++)
            {
                listWriteTo.Add(new ComicImageViewModel(new ComicImage(new PDFPage(_pdfDcoument, index), ImageType.Bmp), index + 1));
            }
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PDFFile() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
