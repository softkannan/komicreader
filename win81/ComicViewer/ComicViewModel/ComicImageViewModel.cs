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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

using ComicViewer.ComicModel;

namespace ComicViewer
{
    public class ComicImageViewModel : INotifyPropertyChanged
    {
        private int pageNo;

        public ComicImageViewModel Next { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
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
        public static Size PageSize { get; set; }
        public ComicViewer.ComicModel.ComicImage Image { get; private set; }
        public virtual int PageNo
        {
            get { return pageNo; }
        }
        public ImageSource ImageData
        {
            get
            {
                UpdateImage();
                return Image.ImageData;
            }
        }

        public ImageSource ImageSrc
        {
            get
            {
                UpdateImage();
                return Image.ImageData;
            }
        }

        private void UpdateImage()
        {
            if (!Image.IsImagePopulated)
            {
                var task = GetImageAsync();

                //Wait for image data to be loaded before we update the image attribute values
                task.Wait();

                UpdateImageAttribute();

                //Now trigger the set of properties values are changed and notify the contorls to pickup the dat in async fashion.
                //we don't need to wait
                task.ContinueWith(async (taskArg) =>
                {
                    await InvalidateData();

                }, TaskScheduler.Current);

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
                InternalPropertyChanged("ImageData");
            });
        }

        public void UpdateImageAttribute()
        {
            Size measureChildSize;
            Size availableSize = new Size(PageSize.Width, PageSize.Height);

            double ratioX = availableSize.Width;
            double ratioY = availableSize.Height;
            // use whichever multiplier is smaller
            double ratio = 1.0;
            // now we can get the new height and width
            int newHeight = Convert.ToInt32(availableSize.Height);
            int newWidth = Convert.ToInt32(availableSize.Width);

            if (Image != null && Image.IsImagePopulated)
            {
                // Figure out the ratio
                ratioX = availableSize.Width / Image.ActualWidth;
                ratioY = availableSize.Height / Image.ActualHeight;
                // use whichever multiplier is smaller
                ratio = ratioX < ratioY ? ratioX : ratioY;

                // now we can get the new height and width
                newHeight = Convert.ToInt32(Image.ActualHeight * ratio);
                newWidth = Convert.ToInt32(Image.ActualWidth * ratio);
            }

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

            Width = measureChildSize.Width;
            Height = measureChildSize.Height;
        }
        public void UnsetImageData()
        {
            if (Image != null)
            {
                Image.Dispose();
            }

            if (Next != null)
            {
                Next.UnsetImageData();
            }

        }
        public async Task<ImageSource> GetImageAsync()
        {
            if(Image.IsImagePopulated)
            {
                return Image.ImageData;
            }

            var retVal = Image.ImageData;

            if (Next != null)
            {
                await Next.GetImageAsync();
            }

            UpdateImageAttribute();

            return retVal;
        }
        
        public ComicImageViewModel(ComicImageViewModel image):this(image.Image,image.PageNo)
        {
            
        }

        public ComicImageViewModel(ComicImage data, int pageNo)
        {
            this.Image = data;
            this.pageNo = pageNo;
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
