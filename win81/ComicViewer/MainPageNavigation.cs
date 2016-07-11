using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ComicViewer
{
    public partial class MainPage
    {
        public float ZoomFactor { get; set; }

        public ComicImages CurrentSource { get; set; }

        int totalPage = 0;

        public int LastPage
        {
            get
            {
                return totalPage;
            }
            set
            {
                totalPage = value;
                InternalPropertyChanged("TotalPage");
            }
        }
        private double pageViewHeight = 0;
        public double PageViewHeight
        {
            get
            {
                return pageViewHeight;
            }
            set
            {
                pageViewHeight = value;
                InternalPropertyChanged("PageViewHeight");
            }
        }

        private double pageViewWidth = 0;
        public double PageViewWidth
        {
            get
            {
                return pageViewWidth;
            }
            set
            {
                pageViewWidth = value;
                InternalPropertyChanged("PageViewWidth");
            }
        }

        void GotoLastPage()
        {
            if (CurrentSource != null)
            {
                if (slimSemaphore.Wait(10))
                {
                    CurrentPage = CurrentSource.Max((item) => item.PageNo);
                    slimSemaphore.Release();
                }
            }
        }

        void GotoFirstPage()
        {
            if (CurrentSource != null)
            {
                if (slimSemaphore.Wait(10))
                {
                    CurrentPage = 1;

                    slimSemaphore.Release();
                }
            }
        }

        void Next()
        {
            if (CurrentSource != null)
            {
                if (slimSemaphore.Wait(10))
                {
                    

                    if (PanelMode == ComicViewer.PanelMode.DoublePage)
                    {
                        CurrentPage += 2;
                    }
                    else
                    {
                        CurrentPage++;
                    }
                    var lastPage = CurrentSource.Max((item) => item.PageNo);
                    if (CurrentPage > lastPage)
                    {
                        CurrentPage = lastPage;
                    }
                    slimSemaphore.Release();
                }
            }
        }

        void Back()
        {
            if (CurrentSource != null)
            {
                if (slimSemaphore.Wait(10))
                {
                    if (PanelMode == ComicViewer.PanelMode.DoublePage)
                    {
                        CurrentPage -= 2;
                    }
                    else
                    {
                        CurrentPage--;
                    }
                    var lastPage = CurrentSource.Min((item) => item.PageNo);
                    if (CurrentPage < lastPage)
                    {
                        CurrentPage = lastPage;
                    }
                    slimSemaphore.Release();
                }
            }
        }

        SemaphoreSlim slimSemaphore = new SemaphoreSlim(1);

        private void UpdateFlipPages()
        {
            if (PanelMode == ComicViewer.PanelMode.SinglePage  && AppSettings.FlipView)
            {
                var frame = CurrentSource.FindIndex((item) => item.PageNo == CurrentPage);

                if (frame != -1)
                {
                    pageFlipView.SelectedIndex = frame;
                }
                else
                {
                    pageFlipView.SelectedIndex = 0;
                }
                pageFlipView.UpdateLayout();
            }
            else if (PanelMode == ComicViewer.PanelMode.DoublePage && AppSettings.FlipView)
            {
                var frame = CurrentSource.FindIndex((item) =>{
                    return item.PageNo == CurrentPage || (item.Next != null && item.Next.PageNo == CurrentPage);
                });

                if (frame != -1)
                {
                    bookFlipView.SelectedIndex = frame;
                }
                else
                {
                    bookFlipView.SelectedIndex = 0;
                }

                bookFlipView.UpdateLayout();
            }
        }

        public async Task ShowPage()
        {

            try
            {
                if (slimSemaphore.Wait(10))
                {
                    if (Pages != null)
                    {
                        CurrentSource = Pages;

                        UpdateComicSettings();

                        ReleaseImage();

                        switch (PanelMode)
                        {
                            case ComicViewer.PanelMode.SinglePage:
                                {
                                    SinglePageUpdateUI();

                                    if (AppSettings.FlipView)
                                    {
                                        pageFlipView.ItemsSource = new ComicSource(CurrentSource);

                                        pageFlipView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                                        UpdateFlipPages();
                                    }
                                    else
                                    {

                                        var frame = CurrentSource.FirstOrDefault((item) => item.PageNo == CurrentPage);

                                        if (frame != null)
                                        {
                                            pageView.ChangeView(null,null,frame.ZoomFactor);
                                        }
                                        pageView.DataContext = frame;
                                        pageView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                                        pageView.UpdateLayout();
                                    }

                                    bttnBack.IsEnabled = true;
                                    bttnNext.IsEnabled = true;
                                }
                                break;
                            case ComicViewer.PanelMode.DoublePage:
                                {

                                   

                                    if (AppSettings.FlipView)
                                    {

                                        CurrentSource = new ComicImages();

                                        ComicImage firstImage = null;

                                        var tempIndex = Pages.FindIndex((item) => item.PageNo == CurrentPage);
                                        int skipCount = tempIndex % 2;

                                        firstImage = Pages.Skip(skipCount).FirstOrDefault();

                                        if (firstImage != null)
                                        {
                                            BookViewUpdateUI(firstImage);

                                            Pages.Skip(1 + skipCount).Exec((item) =>
                                            {
                                                firstImage.Next = new ComicImage(item);
                                                firstImage = item;
                                            });

                                            CurrentSource = new ComicImages();

                                            if (skipCount == 1)
                                            {
                                                CurrentSource.Add(Pages.First());
                                            }

                                            for (int index = skipCount; index < Pages.Count; index += 2)
                                            {
                                                CurrentSource.Add(Pages[index]);
                                            }
                                            
                                        }

                                        bookFlipView.ItemsSource = new ComicSource(CurrentSource);

                                        bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                                        UpdateFlipPages();

                                    }
                                    else
                                    {
                                        ImageSource imgSrc1 = null;
                                        ImageSource imgSrc2 = null;

                                        ComicImage frame1 = null;
                                        ComicImage frame2 = null;

                                        frame1 = CurrentSource.FirstOrDefault((item) => item.PageNo == CurrentPage);
                                        frame2 = CurrentSource.FirstOrDefault((item) => item.PageNo == CurrentPage + 1);

                                        if (frame1 != null)
                                        {
                                            imgSrc1 = await frame1.GetImageAsync();
                                            frame1.UpdateImageAttribute();
                                        }

                                        if (frame2 != null)
                                        {
                                            imgSrc2 = await frame2.GetImageAsync();
                                            frame2.UpdateImageAttribute();
                                        }

                                        if (frame1 != null)
                                        {
                                            BookViewUpdateUI(frame1);
                                        }
                                        else if (frame2 != null)
                                        {
                                            BookViewUpdateUI(frame2);
                                        }

                                        page1.Width = Double.NaN;
                                        page1.Height = Double.NaN;
                                        page2.Width = Double.NaN;
                                        page2.Height = Double.NaN;
                                   

                                        if (frame1 != null)
                                        {
                                            page1.Width = frame1.Width;
                                            page1.Height = frame1.Height;

                                            page1.Stretch = frame1.Stretch;
                                        }

                                        if (frame2 != null)
                                        {
                                            page2.Width = frame2.Width;
                                            page2.Height = frame2.Height;
                                            page2.Stretch = frame2.Stretch;
                                        }

                                        page1.Source = imgSrc1;
                                        page2.Source = imgSrc2;

                                        bookView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                                        bookView.UpdateLayout();
                                    }

                                    bttnBack.IsEnabled = true;
                                    bttnNext.IsEnabled = true;
                                }
                                break;
                            case ComicViewer.PanelMode.ContniousPage:
                                {
                                    ScrollViewUpdateUI();

                                    jumpToPage = CurrentSource.First((item) => item.PageNo == CurrentPage);

                                    continuousView.ItemsSource = new ComicSource(CurrentSource);

                                    bttnBack.IsEnabled = false;
                                    bttnNext.IsEnabled = false;
                                    continuousView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                                    continuousView.UpdateLayout();

                                }
                                break;
                        }

                        UpdateZoomStatus();
                        UpdatePanelModeStatus();
                    }
                }
            }
            finally
            {
                slimSemaphore.Release();
            }

        }

        private void ScrollViewUpdateUI()
        {
            continuousView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            bookView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            bttnFreeForm.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bttnFit.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bttnZoomIn.Visibility = Windows.UI.Xaml.Visibility.Visible;
            bttnZoomOut.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void BookViewUpdateUI(ComicImage item)
        {
            continuousView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if (AppSettings.FlipView)
            {
                bookView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                bttnFreeForm.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnFit.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnFitWidth.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnZoomIn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                bttnZoomOut.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                bookView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                bttnFreeForm.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnFit.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnFitWidth.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnZoomIn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                bttnZoomOut.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            pageView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            //HorizontalScrollBarVisibility="{Binding Path=HorizontalScrollBarVisibility}"
            //              HorizontalScrollMode="{Binding Path=HorizontalScrollMode}" VerticalScrollBarVisibility="{Binding Path=VerticalScrollBarVisibility}"
            //              VerticalScrollMode="{Binding Path=VerticalScrollMode}" ZoomMode="{Binding Path=ZoomMode}"
            bookView.HorizontalScrollBarVisibility = item.HorizontalScrollBarVisibility;
            bookView.HorizontalScrollMode = item.HorizontalScrollMode;
            bookView.VerticalScrollBarVisibility = item.VerticalScrollBarVisibility;
            bookView.VerticalScrollMode = item.VerticalScrollMode;
            bookView.ZoomMode = item.ZoomMode;
            twoPageStackPanel.FlowDirection = item.FlowDirection;

            bookView.ChangeView(null,null,item.ZoomFactor);
        }

        private void UpdateScrollSettings(ComicImage image)
        {
            switch (PanelMode)
            {
                case ComicViewer.PanelMode.DoublePage:
                    {
                        switch (Zoom)
                        {
                            case ZoomType.Fit:
                                image.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                image.HorizontalScrollMode = ScrollMode.Disabled;
                                image.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                image.VerticalScrollMode = ScrollMode.Disabled;
                                image.ZoomMode = ZoomMode.Disabled;
                                image.ZoomFactor = 1;
                                break;
                            case ZoomType.Custom:
                            case ZoomType.FitWidth:
                                image.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                image.HorizontalScrollMode = ScrollMode.Disabled;
                                image.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                                image.VerticalScrollMode = ScrollMode.Enabled;
                                image.ZoomMode = ZoomMode.Disabled;
                                image.ZoomFactor = 1;
                                break;
                            case ZoomType.FreeForm:
                                image.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                                image.HorizontalScrollMode = ScrollMode.Auto;
                                image.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                                image.VerticalScrollMode = ScrollMode.Auto;
                                image.ZoomMode = ZoomMode.Enabled;
                                image.ZoomFactor = 1;
                                break;

                        }
                    }
                    break;
                case ComicViewer.PanelMode.SinglePage:
                    {
                        switch (Zoom)
                        {
                            case ZoomType.Fit:
                                image.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                image.HorizontalScrollMode = ScrollMode.Disabled;
                                image.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                image.VerticalScrollMode = ScrollMode.Disabled;
                                image.ZoomMode = ZoomMode.Disabled;
                                image.ZoomFactor = 1;
                                break;
                            case ZoomType.Custom:
                            case ZoomType.FitWidth:
                                image.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                image.HorizontalScrollMode = ScrollMode.Disabled;
                                image.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                                image.VerticalScrollMode = ScrollMode.Auto;
                                image.ZoomMode = ZoomMode.Disabled;
                                image.ZoomFactor = 1;
                                break;
                            case ZoomType.FreeForm:
                                image.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                                image.HorizontalScrollMode = ScrollMode.Auto;
                                image.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                                image.VerticalScrollMode = ScrollMode.Auto;
                                image.ZoomMode = ZoomMode.Enabled;
                                image.ZoomFactor = 1;
                                break;

                        }
                    }
                    break;
            }
        }
        

        private void SinglePageUpdateUI()
        {
            continuousView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bookView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if (AppSettings.FlipView)
            {
                pageView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                pageFlipView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                bttnFreeForm.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnFit.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnFitWidth.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnZoomIn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                bttnZoomOut.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                pageFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                pageView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                bttnFreeForm.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnFit.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnFitWidth.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bttnZoomIn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                bttnZoomOut.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void UpdateCurrentPage()
        {
            if(PanelMode == ComicViewer.PanelMode.ContniousPage)
            {
                IEnumerable<UIElement> tempObj = VisualTreeHelper.FindElementsInHostCoordinates(new Point(this.ActualWidth / 2, this.ActualHeight / 2), continuousView);

                if (tempObj != null)
                {
                    var tempListViewItem = tempObj.FirstOrDefault((item) => item is ListViewItem) as ListViewItem;

                    if (tempListViewItem != null)
                    {
                        ComicImage tempImage = tempListViewItem.DataContext as ComicImage;

                        if (tempImage != null)
                        {
                            CurrentPage = tempImage.PageNo;
                        }
                    }
                }
            }
            else if (PanelMode == ComicViewer.PanelMode.SinglePage && AppSettings.FlipView)
            {
                var tempPage = pageFlipView.SelectedItem as ComicImage;

                if (tempPage != null)
                {
                    CurrentPage = tempPage.PageNo;
                }
            }
            else if (PanelMode == ComicViewer.PanelMode.DoublePage && AppSettings.FlipView)
            {
                var tempPage = bookFlipView.SelectedItem as ComicImage;

                if (tempPage != null)
                {
                    CurrentPage = tempPage.PageNo;
                }
            }
            else if (PanelMode == ComicViewer.PanelMode.SinglePage)
            {
                var tempPage = pageView.DataContext as ComicImage;

                if (tempPage != null)
                {
                    CurrentPage = tempPage.PageNo;
                }
            }
            else if (PanelMode == ComicViewer.PanelMode.DoublePage)
            {
                var tempPage = bookView.DataContext as ComicImage;

                if (tempPage != null)
                {
                    CurrentPage = tempPage.PageNo;
                }
            }
        }

        public void UpdateComicSettings()
        {

            var tempEffect = (from t in EffectSettings where t.IsEnabled == true select t).ToList();

            Stretch tempStretch = Stretch.Uniform;

            var tempPanelMode = PanelMode;
            var tempRotation = Rotation;
            var tempZoom = Zoom;
            var tempZoomFactor = ZoomFactor;
            var tempFlowDirection =AppSettings.RightToLeft ? FlowDirection.RightToLeft : Windows.UI.Xaml.FlowDirection.LeftToRight;

            CurrentSource.Exec((item) =>
            {
                item.UnSetImageData();
                item.ImageEffects = tempEffect;
                item.Rotation = tempRotation;
                item.Zoom = tempZoom;
                item.PanelMode = tempPanelMode;
                item.ZoomFactor = tempZoomFactor;
                item.Stretch = tempStretch;
                item.Next = null;
                item.FlowDirection = tempFlowDirection;

                UpdateScrollSettings(item);
            });
        }

        private void ReleaseImage()
        {

            if (bookView.DataContext != null)
            {
                page1.Source = null;
                page2.Source = null;
                bookView.DataContext = null;
            }

            if (pageView.DataContext != null)
            {
                pageView.DataContext = null;
            }
            if (continuousView != null)
            {
                continuousView.ItemsSource = null;
            }
            if (bookFlipView.ItemsSource != null)
            {
                bookFlipView.ItemsSource = null;
            }
            if (pageFlipView.ItemsSource != null)
            {
                pageFlipView.ItemsSource = null;
            }
        }


        private void Busy()
        {
            //VisualStateManager.GoToState(this, "Hidden", true);

            if (comicGrid != null)
            {
                comicGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            if (busyGrid != null)
            {
                busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void NotBusy()
        {
            //VisualStateManager.GoToState(this, "Visible", true);

            if (busyGrid != null)
            {
                busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            if (comicGrid != null)
            {
                comicGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

    }
}
