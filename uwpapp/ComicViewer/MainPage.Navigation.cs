using ComicViewer.ComicViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace ComicViewer
{
    public partial class MainPage
    {
#if DEBUG
        private const int NAVIGATION_OPERATION_WAIT_TIME = 30;
#else
        const int NAVIGATION_OPERATION_WAIT_TIME = 10;
#endif
        private SemaphoreSlim navSync = new SemaphoreSlim(1);
        private ComicImageViewModel continousViewJumpToPage = null;
        private bool IgnoreZoomEvent = false;
        private Popup gotoPopup = null;

        #region Page attributes

       
        public ComicImageViewModelList CurrentSource { get; set; }

        public uint LastPage { get => ComicInfo.Inst.LastPage; }

        public double PageViewHeight { get => ComicInfo.Inst.PageViewHeight; }
        public double PageViewWidth { get => ComicInfo.Inst.PageViewWidth; }

        #endregion Page attributes

        #region Page Navigation methods

        private void GotoLastPage()
        {
            if (CurrentSource != null)
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
                {
                    ComicInfo.Inst.CurrentPage = CurrentSource.Max((item) => item.PageNo);
                    navSync.Release();
                }
            }
        }

        private void GotoFirstPage()
        {
            if (CurrentSource != null)
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
                {
                    ComicInfo.Inst.CurrentPage = 1;

                    navSync.Release();
                }
            }
        }

        private void Next()
        {
            if (CurrentSource != null)
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
                {
                    if (ComicInfo.Inst.PanelMode == PanelMode.DoublePage)
                    {
                        ComicInfo.Inst.CurrentPage += 2;
                    }
                    else
                    {
                        ComicInfo.Inst.CurrentPage++;
                    }
                    var lastPage = CurrentSource.Max((item) => item.PageNo);
                    if (ComicInfo.Inst.CurrentPage > lastPage)
                    {
                        ComicInfo.Inst.CurrentPage = lastPage;
                    }
                    navSync.Release();
                }
            }
        }

        private void Back()
        {
            if (CurrentSource != null)
            {
                if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
                {
                    if (ComicInfo.Inst.PanelMode == PanelMode.DoublePage)
                    {
                        ComicInfo.Inst.CurrentPage -= 2;
                    }
                    else
                    {
                        ComicInfo.Inst.CurrentPage--;
                    }
                    var lastPage = CurrentSource.Min((item) => item.PageNo);
                    if (ComicInfo.Inst.CurrentPage < lastPage)
                    {
                        ComicInfo.Inst.CurrentPage = lastPage;
                    }
                    navSync.Release();
                }
            }
        }

        #endregion Page Navigation methods

        public void ShowPage()
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

                        Pages.Exec((item) =>
                        {
                            item.UnsetImageData();
                        });

                        switch (ComicInfo.Inst.PanelMode)
                        {
                            case PanelMode.SinglePage:
                                {
                                    //Update the content controls visibility
                                    BringupSinglePageViewUpdateUI();

                                    //check is this flip view
                                    if (AppSettings.FlipView)
                                    {
                                        pageFlipView.ItemsSource = new ComicImageSimpleListSource(CurrentSource);

                                        pageFlipView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                                        UpdateFlipPages();
                                    }
                                    else
                                    {
                                        //normal single page view load the page
                                        var frame = CurrentSource.FirstOrDefault((item) => item.PageNo == ComicInfo.Inst.CurrentPage);
                                        frame.TriggerImageLoad();
                                        //apply calculated zoom factor
                                        if (frame != null)
                                        {
                                            pageView.ChangeView(0, 0, frame.ZoomFactor);
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

                            case PanelMode.DoublePage:
                                {
                                    //check flip view enabled
                                    if (AppSettings.FlipView)
                                    {
                                        ComicImageViewModel firstImage = null;

                                        var tempIndex = Pages.FindIndex((item) => item.PageNo == ComicInfo.Inst.CurrentPage);
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

                                        bookFlipView.ItemsSource = new ComicImageSimpleListSource(CurrentSource);

                                        bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                                        UpdateFlipPages();
                                    }
                                    else
                                    {
                                        ImageSource imgSrc1 = null;
                                        ImageSource imgSrc2 = null;

                                        ComicImageViewModel frame1 = null;
                                        ComicImageViewModel frame2 = null;

                                        frame1 = CurrentSource.FirstOrDefault((item) => item.PageNo == ComicInfo.Inst.CurrentPage);
                                        frame2 = CurrentSource.FirstOrDefault((item) => item.PageNo == ComicInfo.Inst.CurrentPage + 1);

                                        if (frame1 != null)
                                        {
                                            frame1.TriggerImageLoad();
                                            imgSrc1 = frame1.ImageSrc;
                                            frame1.UpdateImageAttribute();
                                        }

                                        if (frame2 != null)
                                        {
                                            frame2.TriggerImageLoad();
                                            imgSrc2 = frame2.ImageSrc;
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
                                        if (frame1 != null)
                                        {
                                            bookView.ChangeView(0, 0, frame1.ZoomFactor);
                                        }
                                        else if(frame2 != null)
                                        {
                                            bookView.ChangeView(0, 0, frame2.ZoomFactor);
                                        }

                                        bookView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                                        bookView.UpdateLayout();
                                    }

                                    bttnBack.IsEnabled = true;
                                    bttnNext.IsEnabled = true;
                                }
                                break;

                            case PanelMode.ContniousPage:
                                {
                                    BringupContinuousViewUpdateUI();

                                    continousViewJumpToPage = CurrentSource.First((item) => item.PageNo == ComicInfo.Inst.CurrentPage);

                                    continuousView.ItemsSource = new ComicImageSimpleListSource(CurrentSource);
                                    //continuousView.ItemsSource = new ComicImageRandomSource(CurrentSource);

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

            var tempPanelMode = ComicInfo.Inst.PanelMode;
            var tempRotation = ComicInfo.Inst.Rotation;
            var tempZoom = ComicInfo.Inst.Zoom;
            var tempZoomFactor = ComicInfo.Inst.ZoomFactor;
            var tempFlowDirection = AppSettings.RightToLeft ? FlowDirection.RightToLeft : Windows.UI.Xaml.FlowDirection.LeftToRight;
            var tempOpacity = AppSettings.Opacity;

            for (int index=0; index < CurrentSource.Count; index++) 
            {
                var item = CurrentSource[index];
                item.UnsetImageData();
                item.Image.ImageEffects = tempEffect;
                item.Rotation = tempRotation;
                item.Zoom = tempZoom;
                item.PanelMode = tempPanelMode;
                item.ZoomFactor = tempZoomFactor;
                item.Stretch = tempStretch;
                item.FlowDirection = tempFlowDirection;
                item.Opacity = tempOpacity;

                UpdateScrollSettings(item);
            }
        }

        private void UpdateScrollSettings(ComicImageViewModel image)
        {
            switch (ComicInfo.Inst.PanelMode)
            {
                case PanelMode.DoublePage:
                    {
                        switch (ComicInfo.Inst.Zoom)
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

                case PanelMode.SinglePage:
                    {
                        switch (ComicInfo.Inst.Zoom)
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

        #endregion Update Settings methods

        #region Update Page methods

        private void UpdateRotate()
        {
            var currVal = ComicInfo.Inst.Rotation;

            switch (currVal)
            {
                case RotatePage.RotateNormal:
                    ComicInfo.Inst.Rotation = RotatePage.Rotate90;
                    break;

                case RotatePage.Rotate90:
                    ComicInfo.Inst.Rotation = RotatePage.Rotate180;
                    break;

                case RotatePage.Rotate180:
                    ComicInfo.Inst.Rotation = RotatePage.Rotate270;
                    break;

                case RotatePage.Rotate270:
                    ComicInfo.Inst.Rotation = RotatePage.RotateNormal;
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

            switch (ComicInfo.Inst.Zoom)
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

            switch (ComicInfo.Inst.PanelMode)
            {
                case PanelMode.SinglePage:
                    bttnSinglePage.IsChecked = true;
                    break;

                case PanelMode.DoublePage:
                    bttnTwoPage.IsChecked = true;
                    break;

                case PanelMode.ContniousPage:
                    bttnContinuousPage.IsChecked = true;
                    break;
            }

            IgnoreZoomEvent = false;
        }

        private void UpdateFlipPages()
        {
            if (ComicInfo.Inst.PanelMode == PanelMode.SinglePage && AppSettings.FlipView)
            {
                var frame = CurrentSource.FindIndex((item) => item.PageNo == ComicInfo.Inst.CurrentPage);

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
            else if (ComicInfo.Inst.PanelMode == PanelMode.DoublePage && AppSettings.FlipView)
            {
                var frame = CurrentSource.FindIndex((item) =>
                {
                    return item.PageNo == ComicInfo.Inst.CurrentPage || (item.Next != null && item.Next.PageNo == ComicInfo.Inst.CurrentPage);
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

        private void PerformFlipPageAt(Point position)
        {
            try
            {
                //   pixels / (DPI/96.0)

                DisplayInformation dispInfo = DisplayInformation.GetForCurrentView();

                double maxInch = this.ActualWidth / dispInfo.LogicalDpi;

                double inch = position.X / (double)dispInfo.LogicalDpi;

                if (inch > (maxInch - 3.0))
                {
                    try
                    {
                        if (ComicInfo.Inst.CurrentPage == ComicInfo.Inst.LastPage)
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
                            ShowPage();
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
                        if (ComicInfo.Inst.CurrentPage == 1)
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
                            ShowPage();
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
            if (ComicInfo.Inst.PanelMode == PanelMode.ContniousPage)
            {
                IEnumerable<UIElement> tempObj = VisualTreeHelper.FindElementsInHostCoordinates(new Point(this.ActualWidth / 2, this.ActualHeight / 2), continuousView);

                if (tempObj != null)
                {
                    var tempListViewItem = tempObj.FirstOrDefault((item) => item is Image) as Image;

                    if (tempListViewItem != null)
                    {
                        ComicImageViewModel tempImage = tempListViewItem.DataContext as ComicImageViewModel;

                        if (tempImage != null)
                        {
                            ComicInfo.Inst.CurrentPage = tempImage.PageNo;
                        }
                    }
                }
            }
            else if (ComicInfo.Inst.PanelMode == PanelMode.SinglePage && AppSettings.FlipView)
            {
                var tempPage = pageFlipView.SelectedItem as ComicImageViewModel;

                if (tempPage != null)
                {
                    ComicInfo.Inst.CurrentPage = tempPage.PageNo;
                }
            }
            else if (ComicInfo.Inst.PanelMode == PanelMode.DoublePage && AppSettings.FlipView)
            {
                var tempPage = bookFlipView.SelectedItem as ComicImageViewModel;

                if (tempPage != null)
                {
                    ComicInfo.Inst.CurrentPage = tempPage.PageNo;
                }
            }
            else if (ComicInfo.Inst.PanelMode == PanelMode.SinglePage)
            {
                var tempPage = pageView.DataContext as ComicImageViewModel;

                if (tempPage != null)
                {
                    ComicInfo.Inst.CurrentPage = tempPage.PageNo;
                }
            }
            else if (ComicInfo.Inst.PanelMode == PanelMode.DoublePage)
            {
                var tempPage = bookView.DataContext as ComicImageViewModel;

                if (tempPage != null)
                {
                    ComicInfo.Inst.CurrentPage = tempPage.PageNo;
                }
            }

            //InternalPropertyChanged("CurrentPage");
        }

        #endregion Update Page methods

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

            bookView.ChangeView(null, null, item.ZoomFactor);
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

        #endregion Page View methods
    }
}