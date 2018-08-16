using ComicViewer.ComicModel;
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
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicViewer.ComicViewModel
{
    public class ComicImageViewModel : INotifyPropertyChanged
    {
        private uint pageNo;

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
        public float Opacity { get; set; }
        public static Size PageSize { get; set; }
        public ComicViewer.ComicModel.ComicImage Image { get; private set; }
        private volatile bool isImageLoadInProgress = false;
        public bool IsAsyncImageLoadInProgress { get => isImageLoadInProgress; }

        private ComicImageViewModel _dummyImage = null;
        public ComicImageViewModel DummyImage
        {
            get
            {
                if(_dummyImage == null)
                {
                    lock (this)
                    {
                        if (_dummyImage == null)
                        {
                            _dummyImage = new ComicImageViewModel();

                            _dummyImage.pageNo = pageNo;
                            _dummyImage.Image = Image;

                            _dummyImage.HorizontalScrollBarVisibility = HorizontalScrollBarVisibility;
                            _dummyImage.HorizontalScrollMode = HorizontalScrollMode;
                            _dummyImage.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
                            _dummyImage.VerticalScrollMode = VerticalScrollMode;
                            _dummyImage.ZoomMode = ZoomMode;
                            _dummyImage.ZoomFactor = ZoomFactor;
                            _dummyImage.Zoom = Zoom;
                            _dummyImage.PanelMode = PanelMode;
                            _dummyImage.Rotation = Rotation;
                            _dummyImage.FlowDirection = FlowDirection;
                            _dummyImage.Stretch = Stretch;
                            _dummyImage.Width = Width;
                            _dummyImage.Height = Height;
                        }
                    }
                }
                return _dummyImage;
            }
        }

        public virtual uint PageNo
        {
            get { return pageNo; }
        }

        /// <summary>
        /// This property will be used exclusively by Virtualizing Panels
        /// </summary>
        public ImageSource ImageData
        {
            get
            {
                ////if someone already triggred async load then wait
                //if (isImageLoadInProgress)
                //{
                //    WaitForImageLoad();
                //}
                ////if image is not populated then populate on sync mode
                //if (!Image.IsImagePopulated)
                //{
                //    TriggerImageLoad();
                //}

                //imageLoadTask?.Wait();

                //InvalidateData(new string[]{ nameof(Width), nameof(Height), nameof(Stretch)});
               
                return Image.ImageData;
            }
        }
        /// <summary>
        /// This method is used by all scrollable views
        /// </summary>
        public ImageSource ImageSrc
        {
            get
            {
                //if someone already triggred async load then wait
                //if(isImageLoadInProgress)
                //{
                //    WaitForImageLoad();
                //}
                ////if image is not populated then populate on sync mode
                //if (!Image.IsImagePopulated)
                //{
                //    TriggerImageLoad();
                //}

                return Image.ImageData;
            }
        }

        public void InvalidateData(string[] propertyInvalidate = null)
        {
            //Get the UI thread
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            if(propertyInvalidate == null)
            {
                if (!dispatcher.HasThreadAccess)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        InternalPropertyChanged("Width");
                        InternalPropertyChanged("Height");
                        InternalPropertyChanged("Stretch");
                        InternalPropertyChanged("ImageSrc");
                        InternalPropertyChanged("ImageData");
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    InternalPropertyChanged("Width");
                    InternalPropertyChanged("Height");
                    InternalPropertyChanged("Stretch");
                    InternalPropertyChanged("ImageSrc");
                    InternalPropertyChanged("ImageData");
                }
            }
            else
            {
                if (!dispatcher.HasThreadAccess)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (var item in propertyInvalidate)
                        {
                            InternalPropertyChanged(item);
                        }
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    foreach (var item in propertyInvalidate)
                    {
                        InternalPropertyChanged(item);
                    }
                }
            }
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
                ratioX = (availableSize.Width / Image.ActualWidth);
                ratioY = (availableSize.Height / Image.ActualHeight);
                // use whichever multiplier is smaller
                ratio = (ratioX < ratioY ? ratioX : ratioY);

                // now we can get the new height and width
                newHeight = Convert.ToInt32((int)(Image.ActualHeight * ratio));
                newWidth = Convert.ToInt32((int)(Image.ActualWidth * ratio));
            }

            #region Image Size (Zoom Calculation)

            switch (PanelMode)
            {
                case PanelMode.DoublePage:
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

                case PanelMode.SinglePage:
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

                case PanelMode.ContniousPage:
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

            #endregion Image Size (Zoom Calculation)

            Width = measureChildSize.Width;
            Height = measureChildSize.Height;
        }

        private void UpdateModelData()
        {
            if(Image != null)
            {
                Image.Rotation = this.Rotation;
            }
        }

        public void UnsetImageData()
        {
            if (Image != null)
            {
                Image.FreeImageData();
            }

            if (Next != null)
            {
                Next.UnsetImageData();
            }

            _dummyImage = null;
        }

        public bool TriggerImageLoad()
        {
            //if image is already populated or someon else already triggred async load then 
            //return immediatly and wait on ImageSrc / ImageData property
            if (Image.IsImagePopulated || isImageLoadInProgress)
            {
                return false;
            }

            isImageLoadInProgress = true;

            UpdateModelData();

            var retVal = Image.LoadImageFromFile();

            if (Next != null)
            {
                Next.TriggerImageLoad();
            }

            UpdateImageAttribute();

            isImageLoadInProgress = false;

            return true;
        }

        private ComicImageViewModel()
        {
        }

        public ComicImageViewModel(ComicImageViewModel image) : this(image.Image, image.PageNo)
        {
        }
        
        public ComicImageViewModel(ComicImage data, uint pageNo) : this()
        {
            this.Image = data;
            this.pageNo = pageNo;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void InternalPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members
    }
}