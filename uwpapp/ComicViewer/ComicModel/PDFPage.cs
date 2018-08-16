using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace ComicViewer.ComicModel
{
    public class PDFPage : IPageBitmap
    {
        private PdfDocument _pdfDocument;
        private uint _pageNo;

        private static PdfPageRenderOptions _defaultOptions;
        public static void SetDefaultPageSize(Size pageSize)
        {
            _defaultOptions = new PdfPageRenderOptions();
            _defaultOptions.BackgroundColor = Windows.UI.Colors.Beige;
            _defaultOptions.DestinationHeight = (uint)pageSize.Height;
            _defaultOptions.DestinationWidth = (uint)pageSize.Width;
        }

        public PDFPage(PdfDocument pdfDocument, uint pageNo)
        {
            _pdfDocument = pdfDocument;
            _pageNo = pageNo;
        }

        public void RenderImage(InMemoryRandomAccessStream streamToWriteTo, uint width, uint height)
        {
            using (PdfPage _page = _pdfDocument.GetPage(_pageNo))
            {
                PdfPageRenderOptions renderOptions = new PdfPageRenderOptions();
                renderOptions.BackgroundColor = _defaultOptions.BackgroundColor;
                renderOptions.IsIgnoringHighContrast = false;

                Size newSize = ResizeImage(_defaultOptions.DestinationWidth, _defaultOptions.DestinationHeight, _page.Size.Width, _page.Size.Height);
                renderOptions.DestinationWidth = (uint)newSize.Width;
                renderOptions.DestinationHeight = (uint)newSize.Height;

                _page.RenderToStreamAsync(streamToWriteTo, renderOptions).AsTask().Wait();
            }
        }
        public void RenderImage(InMemoryRandomAccessStream streamToWriteTo)
        {
            using (PdfPage _page = _pdfDocument.GetPage(_pageNo))
            {
                PdfPageRenderOptions renderOptions = new PdfPageRenderOptions();
                renderOptions.BackgroundColor = _defaultOptions.BackgroundColor;
                renderOptions.IsIgnoringHighContrast = false;

                Size newSize = ResizeImage(_defaultOptions.DestinationWidth, _defaultOptions.DestinationHeight, _page.Dimensions.ArtBox.Width, _page.Dimensions.ArtBox.Height);
                renderOptions.DestinationWidth = (uint)newSize.Width;
                renderOptions.DestinationHeight = (uint)newSize.Height;

                _page.RenderToStreamAsync(streamToWriteTo, renderOptions).AsTask().Wait();
            }
        }
        private Size ResizeImage(double canvasWidth, double canvasHeight, double originalWidth, double originalHeight)
        {
            // Figure out the ratio
            double ratioX = canvasWidth / originalWidth;
            //double ratioY = canvasHeight / originalHeight;
            // use whichever multiplier is smaller
            double ratio = ratioX;// > ratioY ? ratioX : ratioY;
            // now we can get the new height and width
            Size retVal = new Size();
            retVal.Height = Convert.ToInt32(originalHeight * ratio);
            retVal.Width = Convert.ToInt32(originalWidth * ratio);
            return retVal;
        }
    }
}
