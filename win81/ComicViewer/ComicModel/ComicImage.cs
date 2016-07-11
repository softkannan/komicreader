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

using SharpCompress.Archive;
using SharpCompress.Archive.Rar;
using Softbuild.Media;

using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicViewer.ComicModel
{
    public class ComicImage : IDisposable
    {
        IArchiveEntry archiveData = null;
        ImageSource imageData = null;

        public RotatePage Rotation { get; set; }
        public double ActualWidth { get; private set; }
        public double ActualHeight { get; private set; }
        public static ImageSource DefaultImage { get; private set; }
        public static Size DefaultImageSize { get; private set; }
        public bool IsImagePopulated { get; private set; }

        //public DateTime GarbagedOn { get; private set; }
        //public bool IsMarkedForGarbage { get; private set; }

        //public void MarkForGarbage()
        //{
        //    IsMarkedForGarbage = true;
        //    GarbagedOn = DateTime.Now;
        //}

        public Windows.UI.Xaml.Media.ImageSource ImageData
        {
            get
            {
                if (imageData == null)
                {
                    imageData = GetImageAsync().Result;

                    if(imageData == null)
                    {
                        imageData = DefaultImage;
                    }

                    IsImagePopulated = true;
                }

                return imageData;
            }
        }
        public List<EffectSetting> ImageEffects { get; set; }

        static ComicImage()
        {
            DefaultImageSize = new Size(10, 10);
            var task = SetDefaultImageAsync(DefaultImageSize);
            task.Wait();
        }
        public ComicImage(IArchiveEntry data)
        {
            archiveData = data;
            IsImagePopulated = false;
            Rotation = RotatePage.RotateNormal;
            ActualHeight = DefaultImageSize.Height;
            ActualWidth = DefaultImageSize.Width;
            ImageEffects = new List<EffectSetting>();
        }

        private async Task<ImageSource> GetImageAsync()
        {
            try
            {
                ImageSource retVal = null;
                ActualWidth = Double.PositiveInfinity;
                ActualHeight = Double.PositiveInfinity;

                using (InMemoryRandomAccessStream tempStream = new InMemoryRandomAccessStream())
                {

                    var writer = tempStream.AsStreamForWrite();
                    {
                        //rawData.WriteTo(writer);
                        using (var readStream = archiveData.OpenEntryStream())
                        {
                            while (true)
                            {
                                byte[] buff = new byte[0x400];

                                int count = readStream.Read(buff, 0, 0x400);

                                if (count > 0)
                                {
                                    writer.Write(buff, 0, count);
                                }
                                else
                                {
                                    writer.Flush();
                                    break;
                                }

                            }

                        }
                    }

                    using (var cloneStream = tempStream.CloneStream())
                    {
                        BitmapImage bitmap;

                        if (ImageEffects.Count == 0 && Rotation == RotatePage.RotateNormal)
                        {
                            bitmap = new BitmapImage();

                            bitmap.SetSource(cloneStream);

                            ActualHeight = bitmap.PixelHeight;
                            ActualWidth = bitmap.PixelWidth;

                            retVal = bitmap;
                        }
                        else
                        {
                            var tempBitmap = await BitmapDecoder.CreateAsync(cloneStream);

                            // var frame = await bitmap.GetFrameAsync(0);

                            var pixelData = await tempBitmap.GetPixelDataAsync();

                            var pixels = pixelData.DetachPixelData();

                            WriteableBitmap tempImage = new WriteableBitmap((int)tempBitmap.OrientedPixelWidth, (int)tempBitmap.OrientedPixelHeight);

                            using (Stream writeStream = tempImage.PixelBuffer.AsStream())
                            {
                                writeStream.WriteAsync(pixels, 0, pixels.Length).Wait();
                            }

                            tempImage.Invalidate();

                            ActualWidth = tempImage.PixelWidth;
                            ActualHeight = tempImage.PixelHeight;


                            switch (Rotation)
                            {
                                case RotatePage.Rotate90:
                                    {
                                        var tempData = tempImage;
                                        tempImage = tempData.Rotate(90);
                                    }
                                    break;
                                case RotatePage.Rotate180:
                                    {
                                        var tempData = tempImage;
                                        tempImage = tempData.Rotate(180);
                                    }
                                    break;
                                case RotatePage.Rotate270:
                                    {
                                        var tempData = tempImage;
                                        tempImage = tempData.Rotate(270);
                                    }
                                    break;
                            }

                            foreach (var item in ImageEffects)
                            {
                                var tempData = tempImage;
                                switch (item.Type)
                                {
                                    case ImageEffect.AutoColoring:
                                        {
                                            tempImage = tempData.EffectAutoColoring();
                                        }
                                        break;
                                    case ComicViewer.ImageEffect.Grey:
                                        {
                                            tempImage = tempData.EffectGrayscale();
                                        }
                                        break;
                                    case ImageEffect.Bakumatsu:
                                        {
                                            tempImage = await tempData.EffectBakumatsuAsync();
                                        }
                                        break;
                                    case ImageEffect.Contrast:
                                        {
                                            tempImage = tempData.EffectContrast(item.Value);
                                        }
                                        break;
                                    case ImageEffect.Posterize:
                                        {
                                            tempImage = tempData.EffectPosterize((byte)item.Value);
                                        }
                                        break;

                                }
                            }

                            retVal = tempImage;
                        }
                    }
                }
                return retVal;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public static async Task SetDefaultImageAsync(Size pageSize)
        {
            WriteableBitmap tempImage = new WriteableBitmap((int)pageSize.Width, (int)pageSize.Height);
            DefaultImageSize = pageSize;

            var pixels = new byte[4 * tempImage.PixelWidth * tempImage.PixelHeight];

            // Initialize pixels to white
            for (int index = 0; index < pixels.Length; index++)
                pixels[index] = 0xFF;

            var pixelStream = tempImage.PixelBuffer.AsStream();

            using (Stream writeStream = tempImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(pixels, 0, pixels.Length);
            }

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
                // Free any other managed objects here.
                //
                if (imageData != null)
                {
                    imageData = null;
                }
                IsImagePopulated = false;
            }

            // Free any unmanaged objects here.
            //
        }
        #endregion
    }
}
