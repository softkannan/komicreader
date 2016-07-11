using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace ComicViewer
{
    public partial class MainPage
    {
#if DEBUG
        const int NAVIGATION_OPERATION_WAIT_TIME = 30;
#else
        const int NAVIGATION_OPERATION_WAIT_TIME = 10;
#endif
        int totalPage = 0;
        private double pageViewHeight = 0;
        private double pageViewWidth = 0;
        SemaphoreSlim navSync = new SemaphoreSlim(1);
        ComicImageViewModel continousViewJumpToPage = null;
        bool IgnoreZoomEvent = false;
        Popup gotoPopup = null;

        #region Page attributes
        public float ZoomFactor { get; set; }
        public ComicImageViewModelList CurrentSource { get; set; }
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
        #endregion

        #region Page Navigation methods
        void GotoLastPage()
        {
            if (CurrentSource != null)
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
                {
                    CurrentPage = CurrentSource.Max((item) => item.PageNo);
                    navSync.Release();
                }
            }
        }
        void GotoFirstPage()
        {
            if (CurrentSource != null)
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
                {
                    CurrentPage = 1;

                    navSync.Release();
                }
            }
        }
        void Next()
        {
            if (CurrentSource != null)
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
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
                    navSync.Release();
                }
            }
        }
        void Back()
        {
            if (CurrentSource != null)
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
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
                    navSync.Release();
                }
            }
        }
        #endregion

        public async Task ShowPage()
        {

            try
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
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
                                    //Update the content controls visibility
                                    BringupSinglePageViewUpdateUI();

                                    //check is this flip view
                                    if (AppSettings.FlipView)
                                    {
                                        pageFlipView.ItemsSource = new ComicImageListViewModel(CurrentSource);

                                        pageFlipView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                                        UpdateFlipPages();
                                    }
                                    else
                                    {
                                        //normal single page view load the page
                                        var frame = CurrentSource.FirstOrDefault((item) => item.PageNo == CurrentPage);

                                        //apply calculated zoom factor
                                        if (frame != null)
                                        {
                                            pageView.ChangeView(null,null,frame.ZoomFactor);
                                        }
                                        //set the image data
                                        pageView.DataContext = frame;
                                        //bring up the page visiblity
                                        pageView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                                        pageView.UpdateLayout();
                                    }

                                    bttnBack.IsEnabled = true;
                                    bttnNext.IsEnabled = true;
                                }
                                break;
                            case ComicViewer.PanelMode.DoublePage:
                                {
                                    //check flip view enabled
                                    if (AppSettings.FlipView)
                                    {

                                        CurrentSource = new ComicImageViewModelList();

                                        ComicImageViewModel firstImage = null;

                                        var tempIndex = Pages.FindIndex((item) => item.PageNo == CurrentPage);
                                        int skipCount = tempIndex % 2;

                                        firstImage = Pages.Skip(skipCount).FirstOrDefault();

                                        if (firstImage != null)
                                        {
                                            BringupBookViewUpdateUI(firstImage);

                                            Pages.Skip(1 + skipCount).Exec((item) =>
                                            {
                                                firstImage.Next = new ComicImageViewModel(item);
                                                firstImage = item;
                                            });

                                            CurrentSource = new ComicImageViewModelList();

                                            if (skipCount == 1)
                                            {
                                                CurrentSource.Add(Pages.First());
                                            }

                                            for (int index = skipCount; index < Pages.Count; index += 2)
                                            {
                                                CurrentSource.Add(Pages[index]);
                                            }
                                            
                                        }

                                        bookFlipView.ItemsSource = new ComicImageListViewModel(CurrentSource);

                                        bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                                        UpdateFlipPages();

                                    }
                                    else
                                    {
                                        ImageSource imgSrc1 = null;
                                        ImageSource imgSrc2 = null;

                                        ComicImageViewModel frame1 = null;
                                        ComicImageViewModel frame2 = null;

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
                                            BringupBookViewUpdateUI(frame1);
                                        }
                                        else if (frame2 != null)
                                        {
                                            BringupBookViewUpdateUI(frame2);
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
                                    BringupContinuousViewUpdateUI();

                                    continousViewJumpToPage = CurrentSource.First((item) => item.PageNo == CurrentPage);

                                    continuousView.ItemsSource = new ComicImageListViewModel(CurrentSource);

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
                navSync.Release();
            }

        }

        #region Update Settings methods
        public void UpdateComicSettings()
        {

            var tempEffect = (from t in EffectSettings where t.IsEnabled == true select t).ToList();

            Stretch tempStretch = Stretch.Uniform;

            var tempPanelMode = PanelMode;
            var tempRotation = Rotation;
            var tempZoom = Zoom;
            var tempZoomFactor = ZoomFactor;
            var tempFlowDirection = AppSettings.RightToLeft ? FlowDirection.RightToLeft : Windows.UI.Xaml.FlowDirection.LeftToRight;

            CurrentSource.Exec((item) =>
            {
                item.UnsetImageData();
                item.Image.ImageEffects = tempEffect;
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
        private void UpdateScrollSettings(ComicImageViewModel image)
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
        #endregion

        #region Update Page methods
        private void UpdateRotate()
        {
            var currVal = Rotation;

            switch (currVal)
            {
                case RotatePage.RotateNormal:
                    Rotation = RotatePage.Rotate90;
                    break;
                case RotatePage.Rotate90:
                    Rotation = RotatePage.Rotate180;
                    break;
                case RotatePage.Rotate180:
                    Rotation = RotatePage.Rotate270;
                    break;
                case RotatePage.Rotate270:
                    Rotation = RotatePage.RotateNormal;
                    break;
            }

        }
        private void UpdateZoomStatus()
        {
            IgnoreZoomEvent = true;

            bttnFit.IsChecked = false;
            bttnFitWidth.IsChecked = false;
            //bttnOriginal.IsChecked = false;
            bttnFreeForm.IsChecked = false;

            switch (Zoom)
            {
                case ZoomType.FitWidth:
                    bttnFitWidth.IsChecked = true;
                    break;
                case ZoomType.Fit:
                    bttnFit.IsChecked = true;
                    break;
                case ZoomType.FreeForm:
                    bttnFreeForm.IsChecked = true;
                    break;
            }

            IgnoreZoomEvent = false;
        }
        private void UpdatePanelModeStatus()
        {
            IgnoreZoomEvent = true;

            bttnSinglePage.IsChecked = false;
            bttnTwoPage.IsChecked = false;
            bttnContinuousPage.IsChecked = false;

            switch (PanelMode)
            {
                case ComicViewer.PanelMode.SinglePage:
                    bttnSinglePage.IsChecked = true;
                    break;
                case ComicViewer.PanelMode.DoublePage:
                    bttnTwoPage.IsChecked = true;
                    break;
                case ComicViewer.PanelMode.ContniousPage:
                    bttnContinuousPage.IsChecked = true;
                    break;
            }

            IgnoreZoomEvent = false;
        }
        private void UpdateFlipPages()
        {
            if (PanelMode == ComicViewer.PanelMode.SinglePage && AppSettings.FlipView)
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
                var frame = CurrentSource.FindIndex((item) =>
                {
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
        private async Task PerformFlipPageAt(Point position)
        {
            try
            {
                UpdateCurrentPage();

                //   pixels / (DPI/96.0)

                DisplayInformation dispInfo = DisplayInformation.GetForCurrentView();

                double maxInch = this.ActualWidth / dispInfo.LogicalDpi;

                double inch = position.X / (double)dispInfo.LogicalDpi;

                if (inch > (maxInch - 3.0))
                {
                    try
                    {
                        if (CurrentPage == LastPage)
                        {
                            return;
                        }
                        Busy();
                        Next();

                        if (AppSettings.FlipView)
                        {
                            UpdateFlipPages();
                        }
                        else
                        {
                            await ShowPage();
                        }
                    }
                    finally
                    {
                        NotBusy();
                    }
                }
                else if (inch < 3)
                {
                    try
                    {
                        if (CurrentPage == 1)
                        {
                            return;
                        }

                        Busy();
                        Back();

                        if (AppSettings.FlipView)
                        {
                            UpdateFlipPages();
                        }
                        else
                        {
                            await ShowPage();
                        }
                    }
                    finally
                    {
                        NotBusy();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Page Navigation", ex);
            }
        }
        private void UpdateCurrentPage()
        {
            if (PanelMode == ComicViewer.PanelMode.ContniousPage)
            {
                IEnumerable<UIElement> tempObj = VisualTreeHelper.FindElementsInHostCoordinates(new Point(this.ActualWidth / 2, this.ActualHeight / 2), continuousView);

                if (tempObj != null)
                {
                    var tempListViewItem = tempObj.FirstOrDefault((item) => item is ListViewItem) as ListViewItem;

                    if (tempListViewItem != null)
                    {
                        ComicImageViewModel tempImage = tempListViewItem.DataContext as ComicImageViewModel;

                        if (tempImage != null)
                        {
                            CurrentPage = tempImage.PageNo;
                        }
                    }
                }
            }
            else if (PanelMode == ComicViewer.PanelMode.SinglePage && AppSettings.FlipView)
            {
                var tempPage = pageFlipView.SelectedItem as ComicImageViewModel;

                if (tempPage != null)
                {
                    CurrentPage = tempPage.PageNo;
                }
            }
            else if (PanelMode == ComicViewer.PanelMode.DoublePage && AppSettings.FlipView)
            {
                var tempPage = bookFlipView.SelectedItem as ComicImageViewModel;

                if (tempPage != null)
                {
                    CurrentPage = tempPage.PageNo;
                }
            }
            else if (PanelMode == ComicViewer.PanelMode.SinglePage)
            {
                var tempPage = pageView.DataContext as ComicImageViewModel;

                if (tempPage != null)
                {
                    CurrentPage = tempPage.PageNo;
                }
            }
            else if (PanelMode == ComicViewer.PanelMode.DoublePage)
            {
                var tempPage = bookView.DataContext as ComicImageViewModel;

                if (tempPage != null)
                {
                    CurrentPage = tempPage.PageNo;
                }
            }
        }

        #endregion

        #region Page View methods
        private void BringupContinuousViewUpdateUI()
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

        private void BringupBookViewUpdateUI(ComicImageViewModel item)
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

        private void BringupSinglePageViewUpdateUI()
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
#endregion

    }
}
