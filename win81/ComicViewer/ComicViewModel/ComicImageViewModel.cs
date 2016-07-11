using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.ComponentModel;
using Windows.UI.Xaml;
using SharpCompress.Archive.Rar;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Core;
using Softbuild.Media;
using Windows.UI.Xaml.Controls;
using System.Collections;
using System.Threading;
using Windows.UI.Popups;

namespace ComicViewer
{
    public class ComicImageViewModel : INotifyPropertyChanged
    {

        public ComicImageViewModel Next { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
        public Stretch Stretch { get; set; }
        public ZoomType Zoom { get; set; }
        public PanelMode PanelMode { get; set; }
        public RotatePage Rotation { get; set; }
        public FlowDirection FlowDirection { get; set; }

        public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
        public ScrollMode HorizontalScrollMode { get; set; }
        public ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
        public ScrollMode VerticalScrollMode { get; set; }
        public ZoomMode ZoomMode { get; set; }
        public float ZoomFactor { get; set; }
       

        public List<EffectSetting> ImageEffects { get; set; }

        public static Size PageSize { get; set; }

        IArchiveEntry archiveData;

        ImageSource imageData = null;

        public ImageSource ImageData
        {
            get
            {
                //IsMarkedForGarbage = false;

                if (IsImagePopulated)
                {
                    return imageData;
                }
                else
                {
                    var task = BuildImageAsync();

                    task.ContinueWith(async (taskArg) =>
                    {
                        await InvalidateData();

                    }, TaskScheduler.Current);

                    task.Wait();

                    UpdateImageAttribute();

                    return DefaultImage;
                }
            }
        }

        public ImageSource ImageSrc
        {
            get
            {
                //IsMarkedForGarbage = false;

                if (IsImagePopulated)
                {
                    return imageData;
                }
                else
                {
                    var task = BuildImageAsync();

                    task.ContinueWith(async (taskArg) =>
                    {
                        await InvalidateData();

                    }, TaskScheduler.Current);

                    UpdateImageAttribute();

                    return DefaultImage;
                }
            }
        }

        public async Task InvalidateData()
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InternalPropertyChanged("Width");
                InternalPropertyChanged("Height");
                InternalPropertyChanged("Stretch");
            });

           await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InternalPropertyChanged("ImageSrc");
            });
        }

        public void UpdateImageAttribute()
        {
            Size size = MeasureSize();
            
            Width = size.Width;
            Height = size.Height;
        }

        private Size MeasureSize()
        {

            Size measureChildSize;
            Size availableSize = new Size(PageSize.Width, PageSize.Height);


            // Figure out the ratio
            double ratioX = availableSize.Width / ActualWidth;
            double ratioY = availableSize.Height / ActualHeight;
            // use whichever multiplier is smaller
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            // now we can get the new height and width
            int newHeight = Convert.ToInt32(ActualHeight * ratio);
            int newWidth = Convert.ToInt32(ActualWidth * ratio);

            #region Image Size (Zoom Calculation)
            switch (PanelMode)
            {
                case ComicViewer.PanelMode.DoublePage:
                    switch (Zoom)
                    {
                        case ZoomType.Fit:
                            measureChildSize = new Size(availableSize.Width / 2, availableSize.Height);
                            break;
                        case ZoomType.FreeForm:
                        case ZoomType.FitWidth:
                            measureChildSize = new Size(availableSize.Width / 2, Double.NaN);
                            break;
                        case ZoomType.Custom:
                            var tempZoomFactor = Math.Abs(ZoomFactor);
                            measureChildSize = new Size((availableSize.Width / 2) * tempZoomFactor, newHeight * tempZoomFactor);
                            break;
                    }
                    break;
                case ComicViewer.PanelMode.SinglePage:
                    switch (Zoom)
                    {
                        case ZoomType.Fit:
                            measureChildSize = availableSize;
                            break;
                        case ZoomType.FreeForm:
                        case ZoomType.FitWidth:
                            measureChildSize = new Size(availableSize.Width, Double.NaN);
                            break;
                        case ZoomType.Custom:
                            var tempZoomFactor = Math.Abs(ZoomFactor);
                            measureChildSize = new Size(availableSize.Width * tempZoomFactor, newHeight * tempZoomFactor);
                            break;
                    }
                    break;
                case ComicViewer.PanelMode.ContniousPage:
                    switch (Zoom)
                    {
                        case ZoomType.Fit:
                            measureChildSize = availableSize;
                            break;
                        case ZoomType.FreeForm:
                        case ZoomType.FitWidth:
                            measureChildSize = new Size(availableSize.Width, Double.NaN);
                            break;
                        case ZoomType.Custom:
                            var tempZoomFactor = Math.Abs(ZoomFactor);
                            measureChildSize = new Size(availableSize.Width * tempZoomFactor, Double.NaN);
                            break;
                    }
                    break;
            }
            #endregion

            return measureChildSize;
        }

        //public DateTime GarbagedOn { get; private set; }
        //public bool IsMarkedForGarbage { get; private set; }

        //public void MarkForGarbage()
        //{
        //    IsMarkedForGarbage = true;
        //    GarbagedOn = DateTime.Now;
        //}

        public void UnSetImageData()
        {
            IsImagePopulated = false;
            imageData = null;

            if (Next != null)
            {
                Next.UnSetImageData();
            }

        }

        public static ImageSource DefaultImage { get; private set; }

        public static async Task SetDefaultImageAsync(Size pageSize)
        {
            WriteableBitmap tempImage = new WriteableBitmap((int)pageSize.Width, (int)pageSize.Height);

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

        public bool IsImagePopulated { get; private set; }
       

        public async Task<ImageSource> GetImageAsync()
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
                                await writeStream.WriteAsync(pixels, 0, pixels.Length);
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

        public async Task BuildImageAsync()
        {
            imageData = await GetImageAsync();
            UpdateImageAttribute();
            IsImagePopulated = true;

            if (Next != null)
            {
                await Next.BuildImageAsync();
            }
        }

        public void DeleteImage()
        {
            imageData = null;
            IsImagePopulated = false;
        }

        int pageNo;

        public virtual int PageNo
        {
            get { return pageNo; }
        }
        public ComicImageViewModel(ComicImageViewModel image):this(image.archiveData,image.PageNo)
        {
            this.ImageEffects = image.ImageEffects;
        }

        public ComicImageViewModel(IArchiveEntry data, int pageNo)
        {
            this.archiveData = data;
            this.pageNo = pageNo;
            ImageEffects = new List<EffectSetting>();
            IsImagePopulated = false;
            DefaultImage = null;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void InternalPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
