using SharpCompress.Archive;
using SharpCompress.Archive.Rar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.UI.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ComicViewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            this.InitializeComponent();

            EffectSettings.EffectChanged = async () =>
            {

                try
                {
                    Busy();
                    await ShowPage();
                }
                catch (Exception ex)
                {
                    ShowError("Effect Change", ex);
                    return;
                }
                finally
                {
                    NotBusy();
                }
            
            };

            AppSettings.AppSettingsChanged = async () => {

                try
                {
                    Busy();
                    await ShowPage();
                }
                catch (Exception ex)
                {
                    ShowError("Settings Change", ex);
                    return;
                }
                finally
                {
                    NotBusy();
                }

            };
            
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
            this.SizeChanged += MainPage_SizeChanged;

            Window.Current.CoreWindow.KeyDown += comicGrid_KeyDown;

            continuousView.LayoutUpdated += continuousView_LayoutUpdated;

            this.DataContext = this;
        }

        private async void ShowError(string title,Exception ex)
        {
            MessageDialog message = new MessageDialog(string.Format("{0} error : {1}",title, ex.Message), "Error");
            await message.ShowAsync();
        }

        private async Task ShowErrorAsync(string title, Exception ex)
        {
            MessageDialog message = new MessageDialog(string.Format("{0} error : {1}", title, ex.Message), "Error");
            await message.ShowAsync();
        }

        private async void ShowError(string title,string message)
        {
            MessageDialog messageDia = new MessageDialog(string.Format("{0}{1}",title,message), "Error");
            await messageDia.ShowAsync();
        }

        ComicImage jumpToPage = null;

        void continuousView_LayoutUpdated(object sender, object e)
        {
            try
            {
                if (jumpToPage != null)
                {
                    continuousView.ScrollIntoView(jumpToPage);
                    jumpToPage = null;
                }
            }
            catch (Exception ex)
            {
                ShowError("Continuous View Page Navigation", ex);
            }
        }

        

        void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppSettings != null)
                {
                    AppSettings.SaveSettings();
                }
                CloseComic();
            }
            catch (Exception)
            {
            }
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

            while (true)
            {
                Exception tempEx = null;
                try
                {
                    if (userArgs != null)
                    {
                        CloseComic();

                        Busy();

                        if (userArgs is LaunchActivatedEventArgs)
                        {
                            await OpenComicAsync();
                            userArgs = null;
                        }
                        else
                        {
                            FileActivatedEventArgs args = userArgs as FileActivatedEventArgs;
                            userArgs = null;
                            if (args != null)
                            {
                                await OpenComicAsync(args.Files[0]);
                            }
                        }
                    }
                    else
                    {
                        Busy();
                        await ShowPage();
                    }
                }
                catch (Exception ex)
                {
                    tempEx = ex;
                }
                finally
                {
                    NotBusy();
                }

                if (tempEx != null)
                {
                    await ShowErrorAsync("File Open", tempEx);
                }
                else
                {
                    break;
                }
            }
        }

        async void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                await ShowPage();
            }
            catch (Exception)
            {
            }
            finally
            {
            }
        }

        object userArgs = null;

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                userArgs = e.Parameter;

                this.Loaded += PageLoaded;
                this.Unloaded += PageUnloaded;
            }
            catch (Exception ex)
            {
                ShowError("Komic Launch", ex);
            }
        }
        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width <= 500)
            {
                //VisualStateManager.GoToState(this, state.State, transitions);
            }
            else if (e.Size.Height > e.Size.Width)
            {
                //VisualStateManager.GoToState(this, state.State, transitions);
            }
            else
            {
                //VisualStateManager.GoToState(this, state.State, transitions);
            }
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            ComicImage.PageSize = availableSize;

            PageViewHeight = availableSize.Height;
            PageViewWidth = availableSize.Width;

            var asyncTask = ComicImage.SetDefaultImageAsync(availableSize);

            return base.MeasureOverride(availableSize);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void InternalPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


        private async void bttnOpen_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            while (true)
            {
                Exception tempEx = null;

                try
                {
                    Busy();

                    await OpenComicAsync();
                }
                catch (Exception ex)
                {
                    tempEx = ex;
                }
                finally
                {
                    NotBusy();
                }

                if (tempEx != null)
                {
                    await ShowErrorAsync("File Open", tempEx);
                }
                else
                {
                    break;
                }
            }
        }

        private async void comicGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (AppSettings.MouseFlipType != MousePageFlipType.Single)
            {
                return;
            }

            if (PanelMode == ComicViewer.PanelMode.ContniousPage || AppSettings.FlipView)
            {
                return;
            }

            e.Handled = true;

            Point position = e.GetPosition(this);

            await PerformFlipPageAt(position);
        }

        private async void comicGrid_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (AppSettings.MouseFlipType != MousePageFlipType.Double)
            {
                return;
            }

            if (PanelMode == ComicViewer.PanelMode.ContniousPage || AppSettings.FlipView)
            {
                return;
            }

            e.Handled = true;

            Point position = e.GetPosition(this);

            await PerformFlipPageAt(position);
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

        private async void bttnNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
            catch (Exception ex)
            {
                ShowError("Page Navigation", ex);
            }
            finally
            {
                NotBusy();
            }
        }

        private async void bttnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
            catch (Exception ex)
            {
                ShowError("Page Navigation", ex);
            }
            finally
            {
                NotBusy();
            }
        }

        private async void Zoom_Checked(object sender, RoutedEventArgs e)
        {
            if (IgnoreZoomEvent)
            {
                return;
            }

            

            var toggleButton = sender as ToggleButton;
            
            if (toggleButton != null)
            {
                try
                {
                    if (slimSemaphore.Wait(10))
                    {
                        switch (toggleButton.Name)
                        {
                            case "bttnFit":
                                Zoom = ZoomType.Fit;
                                break;
                            case "bttnFitWidth":
                                Zoom = ZoomType.FitWidth;
                                break;
                            case "bttnFreeForm":
                                Zoom = ZoomType.FreeForm;
                                break;

                        }
                    }
                }
                finally
                {
                    slimSemaphore.Release();
                }

                try
                {
                    Busy();


                    //UpdateZoomStatus();
                    await ShowPage();
                }
                catch (Exception ex)
                {
                    ShowError("Zoom", ex);
                }
                finally
                {
                    NotBusy();
                }
            }

        }

        bool IgnoreZoomEvent = false;

        void UpdateZoomStatus()
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

        void UpdatePanelModeStatus()
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

        private void pageFlipView_CleanUpVirtualizedItemEvent(object sender, CleanUpVirtualizedItemEventArgs e)
        {
            ComicImage image = e.Value as ComicImage;

            if (image != null)
            {
                image.UnSetImageData();
            }

        }

        private void bookFlipView_CleanUpVirtualizedItemEvent(object sender, CleanUpVirtualizedItemEventArgs e)
        {
            ComicImage image = e.Value as ComicImage;

            if (image != null)
            {
                image.UnSetImageData();
            }

            //var uiEle = e.UIElement as FlipViewItem;

            //if (uiEle != null && uiEle.IsSelected == false)
            //{
            //    var border = uiEle.GetElement<Border>();

            //    var contentPre = border.Child as ContentPresenter;

            //    var scrollViewer = contentPre.GetElement<ScrollViewer>();

            //    if (scrollViewer != null)
            //    {
            //        scrollViewer.ZoomToFactor(1);
            //    }

            //}
        }

        private void scrollView_CleanUpVirtualizedItemEvent(object sender, CleanUpVirtualizedItemEventArgs e)
        {
            ComicImage image = e.Value as ComicImage;

            if (image != null)
            {
                image.UnSetImageData();
            }


            //return;

            //if (AppSettings.CachePages == 0)
            //{
            //    if (image != null)
            //    {
            //        image.UnSetImageData();
            //    }
            //}
            //else
            //{

            //    if (image != null)
            //    {
            //        image.MarkForGarbage();
            //    }

            //    var count = Pages.Count((item) => item.IsMarkedForGarbage == true);

            //    if (count > AppSettings.CachePages)
            //    {
            //        //testQuery = (from t in Pages where t.IsMarkedForGarbage == true orderby t.GarbagedOn select t).ToList();

            //        var query = (from t in Pages where t.IsMarkedForGarbage == true orderby t.GarbagedOn descending select t).Skip(AppSettings.CachePages).ToList();

            //        foreach (var item in query)
            //        {
            //            item.UnSetImageData();
            //        }
            //    }
            //}
        }

        private async void PanelMode_Checked(object sender, RoutedEventArgs e)
        {
            if (IgnoreZoomEvent)
            {
                return;
            }
            var toggleButton = sender as ToggleButton;

            if (toggleButton != null)
            {
                try
                {
                    if (slimSemaphore.Wait(10))
                    {
                        switch (toggleButton.Name)
                        {
                            case "bttnSinglePage":
                                PanelMode = ComicViewer.PanelMode.SinglePage;
                                break;
                            case "bttnTwoPage":
                                PanelMode = ComicViewer.PanelMode.DoublePage;
                                break;
                            case "bttnContinuousPage":
                                PanelMode = ComicViewer.PanelMode.ContniousPage;
                                if (Zoom == ZoomType.Fit || Zoom == ZoomType.FreeForm)
                                {
                                    Zoom = ZoomType.FitWidth;
                                    UpdateZoomStatus();
                                }
                                break;
                        }
                    }
                }
                finally
                {
                    slimSemaphore.Release();
                }

                try
                {
                    Busy();
                    //UpdatePanelModeStatus();
                    await ShowPage();
                }
                catch (Exception ex)
                {
                    ShowError("Panel Mode", ex);
                }
                finally
                {
                    NotBusy();
                }
            }
        }

     

        private async void bttnRotate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Busy();
                UpdateRotate();
                await ShowPage();
            }
            catch (Exception ex)
            {
                ShowError("Page Rotate", ex);
            }
            finally
            {
                NotBusy();
            }
        }

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

        Popup gotoPopup = null;

        private void bttnGoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                gotoPopup = new Popup();

                gotoPopup.Closed += gotoPopup_Closed;
                Window.Current.Activated += Current_Activated;
                gotoPopup.IsLightDismissEnabled = true;
                GoToPage gotoPage = new GoToPage() { Width = this.ActualWidth, Height = this.ActualHeight / 2 };
                gotoPage.GotoPage = async (pageNo) =>
                {
                    CurrentPage = pageNo;
                    await ShowPage();
                };

                // Add the proper animation for the panel.
                gotoPopup.ChildTransitions = new TransitionCollection();
                gotoPopup.ChildTransitions.Add(new PaneThemeTransition() { Edge = EdgeTransitionLocation.Bottom });

                // Place the SettingsFlyout inside our Popup window.
                gotoPopup.Child = gotoPage;

                // Let's define the location of our Popup.
                gotoPopup.SetValue(Canvas.LeftProperty, 0);
                gotoPopup.SetValue(Canvas.TopProperty, 0);

                gotoPopup.IsOpen = true;
                gotoPage.SetPages(Pages,Bookmarks,CurrentPage);
            }
            catch (Exception ex)
            {
                ShowError("Goto", ex);
            }
        }

        void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                if (gotoPopup != null)
                {
                    gotoPopup.IsOpen = false;
                    gotoPopup = null;
                }
            }
        }

        void gotoPopup_Closed(object sender, object e)
        {
            Window.Current.Activated -= Current_Activated;
        }

        private async void bttnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom != ZoomType.Custom)
            {
                Zoom = ZoomType.Custom;
            }

            ZoomFactor += AppSettings.ZoomStep;

            if (ZoomFactor > AppSettings.ZoomMax)
            {
                ZoomFactor = AppSettings.ZoomMax;
            }

            try
            {
                await ShowPage();
            }
            catch (Exception ex)
            {
                ShowError("ZoomIn", ex);
            }
        }

        private async void bttnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom != ZoomType.Custom)
            {
               Zoom = ZoomType.Custom;
            }

            ZoomFactor -= AppSettings.ZoomStep;

            if (ZoomFactor < AppSettings.ZoomMin)
            {
                ZoomFactor = AppSettings.ZoomMin;
            }
            try
            {
                await ShowPage();
            }
            catch (Exception ex)
            {
                ShowError("ZoomOut", ex);
            }
        }

        private async void comicGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.Right || 
                e.VirtualKey == Windows.System.VirtualKey.PageDown || e.VirtualKey == Windows.System.VirtualKey.PageUp ||
                e.VirtualKey == Windows.System.VirtualKey.Home || e.VirtualKey == Windows.System.VirtualKey.End)
            {
                e.Handled = true;
                if (PanelMode == ComicViewer.PanelMode.ContniousPage)
                {
                    return;
                }

                try
                {
                    UpdateCurrentPage();

                    Busy();
                    if (e.VirtualKey == Windows.System.VirtualKey.Right || e.VirtualKey == Windows.System.VirtualKey.PageDown)
                    {
                        if (CurrentPage == LastPage)
                        {
                            return;
                        }
                        Next();
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.PageUp)
                    {
                        if (CurrentPage == 1)
                        {
                            return;
                        }
                        Back();
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.Home)
                    {
                        if (CurrentPage == 1)
                        {
                            return;
                        }
                        GotoFirstPage();
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.End)
                    {
                        if (CurrentPage == LastPage)
                        {
                            return;
                        }
                        GotoLastPage();
                    }

                    if (AppSettings.FlipView && CurrentPage != 1 && CurrentPage != LastPage)
                    {
                        UpdateFlipPages();
                    }
                    else
                    {
                        await ShowPage();
                    }
                }
                catch (Exception ex)
                {
                    ShowError("Page Navigation", ex);
                }
                finally
                {
                    NotBusy();
                }
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Down)
            {
                
                if (Zoom != ZoomType.Fit)
                {
                    
                    if (AppSettings.FlipView)
                    {
                       
                    }
                    else
                    {
                        e.Handled = true;
                        if (PanelMode == ComicViewer.PanelMode.SinglePage)
                        {
                            pageView.ChangeView(null,pageView.VerticalOffset + 30,null);
                        }
                        else if (PanelMode == ComicViewer.PanelMode.DoublePage)
                        {
                            bookView.ChangeView(null,bookView.VerticalOffset + 30,null);
                        }
                    }
                }
            }
            else if(e.VirtualKey == Windows.System.VirtualKey.Up)
            {
                
                if (Zoom != ZoomType.Fit)
                {
                    
                    if (AppSettings.FlipView)
                    {
                        if (PanelMode == ComicViewer.PanelMode.SinglePage)
                        {
                        }
                        else if (PanelMode == ComicViewer.PanelMode.DoublePage)
                        {
                        }
                    }
                    else
                    {
                        e.Handled = true;
                        if (PanelMode == ComicViewer.PanelMode.SinglePage)
                        {
                            pageView.ChangeView(null,pageView.VerticalOffset - 30,null);
                        }
                        else if (PanelMode == ComicViewer.PanelMode.DoublePage)
                        {
                            bookView.ChangeView(null,bookView.VerticalOffset - 30,null);
                        }
                    }
                }
            }
        }

        private void TopAppBar_Opened(object sender, object e)
        {
            UpdateCurrentPage();

            if (!string.IsNullOrWhiteSpace(FileName))
            {
                SaveSettings(FileName);
            }
        }

        private async void bookFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                ComicImage tempImage = item as ComicImage;

                if (tempImage != null)
                {
                    tempImage.UnSetImageData();
                    await tempImage.InvalidateData();
                }
            }
        }

        private async void pageFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            foreach (var item in e.RemovedItems)
            {
                ComicImage tempImage = item as ComicImage;

                if (tempImage != null)
                {
                    tempImage.UnSetImageData();
                    await tempImage.InvalidateData();
                }
            }

        }

        
    }
}
