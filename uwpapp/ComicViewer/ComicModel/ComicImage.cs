using ComicViewer.ComicViewModel;
using ComicViewer.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicViewer.ComicModel
{
    public class ComicImage : IDisposable
    {
        private IPageBitmap _pageBitmap = null;
        private ImageType imageType = ImageType.None;
        private ImageSource imageData = null;

        public RotatePage Rotation { get; set; }
        public double ActualWidth { get; private set; }
        public double ActualHeight { get; private set; }
        public static ImageSource DefaultImage { get; private set; }
        public static Size DefaultImageSize { get; private set; }

        private volatile bool _isImagePopulated = false;
        public bool IsImagePopulated { get => _isImagePopulated; }
        
        //public DateTime GarbagedOn { get; private set; }
        //public bool IsMarkedForGarbage { get; private set; }

        //public void MarkForGarbage()
        //{
        //    IsMarkedForGarbage = true;
        //    GarbagedOn = DateTime.Now;
        //}

        /// <summary>
        /// This method is provided to avoid async UI thread dead lock
        /// </summary>
        /// <returns></returns>
        public ImageSource LoadImageFromFile()
        {
            if (imageData == null)
            {
                imageData = LoadImageFromFileInternal();

                if (imageData == null)
                {
                    imageData = DefaultImage;
                }

                _isImagePopulated = true;
            }

            return imageData;
        }

        public Windows.UI.Xaml.Media.ImageSource ImageData
        {
            get
            {
                //if (imageData == null)
                //{
                //    LoadImageFromFile();
                //}
                return imageData;
            }
        }

        public List<EffectSetting> ImageEffects { get; set; }

        static ComicImage()
        {
            DefaultImageSize = new Size(10, 10);
            CreateDefaultImage(DefaultImageSize);
        }

        public ComicImage(IPageBitmap pageBitmap, ImageType imageType)
        {
            _pageBitmap = pageBitmap;
            _isImagePopulated = false;
            Rotation = RotatePage.RotateNormal;
            ActualHeight = DefaultImageSize.Height;
            ActualWidth = DefaultImageSize.Width;
            ImageEffects = new List<EffectSetting>();
            this.imageType = imageType;
        }

        private ImageSource LoadImageFromFileInternal()
        {
            try
            {
                ImageSource retVal = null;
                ActualWidth = Double.PositiveInfinity;
                ActualHeight = Double.PositiveInfinity;

                using (InMemoryRandomAccessStream imageStream = new InMemoryRandomAccessStream())
                {
                    _pageBitmap.RenderImage(imageStream);
                    imageStream.Seek(0);

                    BitmapImage imageSource = null;

                    if (ImageEffects.Count == 0 && Rotation == RotatePage.RotateNormal)
                    {
                        imageSource = new BitmapImage();
                        if (imageType == ImageType.Webp)
                        {
                            /*
                            // Create an instance of the decoder
                            var webp = new WebPDecoder();

                            // Decode to BGRA (Bitmaps use this format)
                            var pixelData = (await webp.DecodeBgraAsync(bytes)).ToArray();

                            // Get the size
                            var size = await webp.GetSizeAsync(bytes);

                            // With the size of the webp, create a WriteableBitmap
                            var bitmap = new WriteableBitmap((int) size.Width, (int) size.Height);

                            // Write the pixel data to the buffer
                            var stream = bitmap.PixelBuffer.AsStream();
                            await stream.WriteAsync(pixelData, 0, pixelData.Length);
                            */
                        }
                        else
                        {
                            imageSource.SetSource(imageStream);
                        }

                        ActualHeight = imageSource.PixelHeight;
                        ActualWidth = imageSource.PixelWidth;

                        retVal = imageSource;
                    }
                    else
                    {
                        WriteableBitmap bitmapImage = null;
                        // create bitmap image
                        {
                            imageSource = new BitmapImage();
                            if (imageType == ImageType.Webp)
                            {
                            }
                            else
                            {
                                imageSource.SetSource(imageStream);
                            }

                            bitmapImage = BitmapFactory.New(imageSource.PixelWidth, imageSource.PixelHeight);
                            imageStream.Seek(0);
                            bitmapImage.SetSource(imageStream);

                            ActualHeight = imageSource.PixelHeight;
                            ActualWidth = imageSource.PixelWidth;
                        }

                        using (BitmapContext ctx = new BitmapContext(bitmapImage))
                        {

                            ActualWidth = bitmapImage.PixelWidth;
                            ActualHeight = bitmapImage.PixelHeight;

                            switch (Rotation)
                            {
                                case RotatePage.Rotate90:
                                    {
                                        var tempData = bitmapImage;
                                        bitmapImage = tempData.Rotate(90);
                                    }
                                    break;

                                case RotatePage.Rotate180:
                                    {
                                        var tempData = bitmapImage;
                                        bitmapImage = tempData.Rotate(180);
                                    }
                                    break;

                                case RotatePage.Rotate270:
                                    {
                                        var tempData = bitmapImage;
                                        bitmapImage = tempData.Rotate(270);
                                    }
                                    break;
                            }

                            //TODO: Writable Bitmap Effect feature needs to fixed

                            foreach (var item in ImageEffects)
                            {
                                var tempData = bitmapImage;
                                switch (item.Type)
                                {
                                    case ImageEffect.Invert:
                                        {
                                            bitmapImage = tempData.Invert();
                                        }
                                        break;

                                    case ImageEffect.Grey:
                                        {
                                            bitmapImage = tempData.Gray();
                                        }
                                        break;
                                    case ImageEffect.Contrast:
                                        {
                                            bitmapImage = tempData.AdjustContrast(item.Value);
                                        }
                                        break;

                                    case ImageEffect.Brightness:
                                        {
                                            bitmapImage = tempData.AdjustBrightness((int)item.Value);
                                        }
                                        break;
                                    case ImageEffect.Flip:
                                        {
                                            WriteableBitmapExtensions.FlipMode flipMode = (WriteableBitmapExtensions.FlipMode) item.Value;
                                            bitmapImage = tempData.Flip(flipMode);
                                        }
                                        break;
                                    case ImageEffect.Convolute:
                                        {
                                            try
                                            {
                                                int[,] kernel = null;
                                                ImageKernel kernelType = (ImageKernel)item.Value;

                                                switch (kernelType)
                                                {
                                                    default:
                                                    case ImageKernel.Blur:
                                                        kernel = ConvolutionKernels.Blur;
                                                        break;
                                                    case ImageKernel.EdgeDetect:
                                                        kernel = ConvolutionKernels.EdgeDetect;
                                                        break;
                                                    case ImageKernel.Emboss:
                                                        kernel = ConvolutionKernels.Emboss;
                                                        break;
                                                    case ImageKernel.Gradient:
                                                        kernel = ConvolutionKernels.Gradient;
                                                        break;
                                                    case ImageKernel.Sharpen:
                                                        kernel = ConvolutionKernels.Sharpen;
                                                        break;
                                                }

                                                bitmapImage = tempData.Convolute(kernel);

                                            }
                                            catch
                                            { }

                                        }
                                        break;
                                }
                            }
                        }

                        retVal = bitmapImage;
                    }
                }
                return retVal;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void CreateDefaultImage(Size pageSize)
        {
            WriteableBitmap tempImage = new WriteableBitmap((int)pageSize.Width, (int)pageSize.Height);
            DefaultImageSize = pageSize;

            //tempImage.FillRectangle(0, 0, (int)pageSize.Width, (int)pageSize.Height, Colors.White);
            tempImage.Clear(Colors.White);

            tempImage.Invalidate();
            DefaultImage = tempImage;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (imageData == null)
                return;

            if (disposing)
            {
                FreeImageData();
            }

            System.GC.SuppressFinalize(this);

            // Free any unmanaged objects here.
            //
        }

        public void FreeImageData()
        {
            // Free any other managed objects here.
            //
            if (imageData != null)
            {
                imageData = null;
            }
            _isImagePopulated = false;
        }

        #endregion IDisposable
    }
}
 