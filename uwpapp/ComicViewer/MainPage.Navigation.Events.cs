using ComicViewer.ComicViewModel;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace ComicViewer
{
    public partial class MainPage
    {
        #region Navigate / Goto methods

        private void bttnNext_Click(object sender, RoutedEventArgs e)
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
                    ShowPage();
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

        private void bttnBack_Click(object sender, RoutedEventArgs e)
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
                    ShowPage();
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

        private void bttnGoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                gotoPopup = new Popup();

                gotoPopup.Closed += gotoPopup_Closed;
                Window.Current.Activated += Current_Activated;
                gotoPopup.IsLightDismissEnabled = true;
                GoToPage gotoPage = new GoToPage() { Width = this.ActualWidth, Height = this.ActualHeight / 2 };
                gotoPage.GotoPage = (pageNo) =>
                {
                    ComicInfo.Inst.CurrentPage = pageNo;
                    ShowPage();
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
                gotoPage.SetPages(Pages, ComicInfo.Inst.Bookmarks, ComicInfo.Inst.CurrentPage);
            }
            catch (Exception ex)
            {
                ShowError("Goto", ex);
            }
        }

        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
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

        private void gotoPopup_Closed(object sender, object e)
        {
            Window.Current.Activated -= Current_Activated;
        }

        #endregion Navigate / Goto methods

        #region Rotate and Zoom methods

        private void bttnRotate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Busy();
                UpdateRotate();
                ShowPage();
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

        

        private void bttnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (ComicInfo.Inst.Zoom != ZoomType.Custom)
            {
                ComicInfo.Inst.Zoom = ZoomType.Custom;
            }

            ComicInfo.Inst.ZoomFactor += AppSettings.ZoomStep;

            if (ComicInfo.Inst.ZoomFactor > AppSettings.ZoomMax)
            {
                ComicInfo.Inst.ZoomFactor = AppSettings.ZoomMax;
            }

            try
            {
                ShowPage();
            }
            catch (Exception ex)
            {
                ShowError("ZoomIn", ex);
            }
        }

        private void bttnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (ComicInfo.Inst.Zoom != ZoomType.Custom)
            {
                ComicInfo.Inst.Zoom = ZoomType.Custom;
            }

            ComicInfo.Inst.ZoomFactor -= AppSettings.ZoomStep;

            if (ComicInfo.Inst.ZoomFactor < AppSettings.ZoomMin)
            {
                ComicInfo.Inst.ZoomFactor = AppSettings.ZoomMin;
            }
            try
            {
                ShowPage();
            }
            catch (Exception ex)
            {
                ShowError("ZoomOut", ex);
            }
        }

        private void Zoom_Checked(object sender, RoutedEventArgs e)
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
                    if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
                    {
                        switch (toggleButton.Name)
                        {
                            case "bttnFit":
                                ComicInfo.Inst.Zoom = ZoomType.Fit;
                                break;

                            case "bttnFitWidth":
                                ComicInfo.Inst.Zoom = ZoomType.FitWidth;
                                break;

                            case "bttnFreeForm":
                                ComicInfo.Inst.Zoom = ZoomType.FreeForm;
                                break;
                        }
                    }
                }
                finally
                {
                    navSync.Release();
                }

                try
                {
                    Busy();

                    //UpdateZoomStatus();
                    ShowPage();
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

        #endregion Rotate and Zoom methods

        #region Title Bar Customization

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                appTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                appTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            //TitleBarButton.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);

            // Update title bar control size as needed to account for system size changes.
            appTitleBar.Height = coreTitleBar.Height;
            RightMask.Width = coreTitleBar.SystemOverlayRightInset;
        }

        private void bttnToggleFullScreen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Busy();
                ToggleFullScreen();
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

        private void ToggleFullScreen()
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        private void CoreWindow_SizeChanged(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.WindowSizeChangedEventArgs args)
        {
            UpdateFullscreenButtonStatus();
        }

        private void UpdateFullscreenButtonStatus()
        {
            var view = ApplicationView.GetForCurrentView();
            ComicInfo.Inst.IsFullScreen = view.IsFullScreenMode;
            if (view.IsFullScreenMode)
            {
                bttnToggleFullScreen.Visibility = Visibility.Collapsed;
                //bttnBackToWindow.Visibility = Visibility.Visible;
            }
            else
            {
                bttnToggleFullScreen.Visibility = Visibility.Visible;
                //bttnBackToWindow.Visibility = Visibility.Collapsed;
            }
        }

        private void bttnShowAppBar_Click(object sender, RoutedEventArgs e)
        {
            UpdateAppbar(!TopAppBar.IsOpen);
        }
        private void UpdateAppbar(bool visiblity)
        {
            if (visiblity)
            {
                //TopAppBar.Visibility = Visibility.Visible;
                //BottomAppBar.Visibility = Visibility.Visible;
                TopAppBar.IsOpen = true;
                BottomAppBar.IsOpen = true;
            }
            else
            {
                TopAppBar.IsOpen = false;
                BottomAppBar.IsOpen = false;
            }
        }

        #endregion

        #region Grid / Tap / Keydown / Top Level methods

        private void PanelMode_Checked(object sender, RoutedEventArgs e)
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
                    if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
                    {
                        switch (toggleButton.Name)
                        {
                            case "bttnSinglePage":
                                ComicInfo.Inst.PanelMode = PanelMode.SinglePage;
                                break;

                            case "bttnTwoPage":
                                ComicInfo.Inst.PanelMode = PanelMode.DoublePage;
                                break;

                            case "bttnContinuousPage":
                                ComicInfo.Inst.PanelMode = PanelMode.ContniousPage;
                                if (ComicInfo.Inst.Zoom == ZoomType.Fit || ComicInfo.Inst.Zoom == ZoomType.FreeForm)
                                {
                                    ComicInfo.Inst.Zoom = ZoomType.FitWidth;
                                    UpdateZoomStatus();
                                }
                                break;
                        }
                    }
                }
                finally
                {
                    navSync.Release();
                }

                try
                {
                    Busy();
                    //UpdatePanelModeStatus();
                    ShowPage();
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

        private void TopAppBar_Opened(object sender, object e)
        {
            ComicInfo.Inst.SaveSettings();
        }

        private void comicGrid_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (AppSettings.MouseFlipType != MousePageFlipType.Double)
            {
                return;
            }

            if (ComicInfo.Inst.PanelMode == PanelMode.ContniousPage || AppSettings.FlipView)
            {
                return;
            }

            e.Handled = true;

            Point position = e.GetPosition(this);

            PerformFlipPageAt(position);
        }

        private void comicGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (AppSettings.MouseFlipType != MousePageFlipType.Single)
            {
                return;
            }

            if (ComicInfo.Inst.PanelMode == PanelMode.ContniousPage || AppSettings.FlipView)
            {
                return;
            }

            e.Handled = true;

            Point position = e.GetPosition(this);

            PerformFlipPageAt(position);
        }

        private void CoreWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.Right ||
                e.VirtualKey == Windows.System.VirtualKey.PageDown || e.VirtualKey == Windows.System.VirtualKey.PageUp ||
                e.VirtualKey == Windows.System.VirtualKey.Home || e.VirtualKey == Windows.System.VirtualKey.End)
            {
                e.Handled = true;
                if (ComicInfo.Inst.PanelMode == PanelMode.ContniousPage)
                {
                    return;
                }

                try
                {
                    Busy();
                    if (e.VirtualKey == Windows.System.VirtualKey.Right || e.VirtualKey == Windows.System.VirtualKey.PageDown)
                    {
                        if (ComicInfo.Inst.CurrentPage == ComicInfo.Inst.LastPage)
                        {
                            return;
                        }
                        Next();
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.PageUp)
                    {
                        if (ComicInfo.Inst.CurrentPage == 1)
                        {
                            return;
                        }
                        Back();
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.Home)
                    {
                        if (ComicInfo.Inst.CurrentPage == 1)
                        {
                            return;
                        }
                        GotoFirstPage();
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.End)
                    {
                        if (ComicInfo.Inst.CurrentPage == ComicInfo.Inst.LastPage)
                        {
                            return;
                        }
                        GotoLastPage();
                    }

                    if (AppSettings.FlipView && ComicInfo.Inst.CurrentPage != 1 && ComicInfo.Inst.CurrentPage != ComicInfo.Inst.LastPage)
                    {
                        UpdateFlipPages();
                    }
                    else
                    {
                        ShowPage();
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
                if (ComicInfo.Inst.Zoom != ZoomType.Fit)
                {
                    if (AppSettings.FlipView)
                    {
                    }
                    else
                    {
                        e.Handled = true;
                        if (ComicInfo.Inst.PanelMode == PanelMode.SinglePage)
                        {
                            pageView.ChangeView(null, pageView.VerticalOffset + 30, null);
                        }
                        else if (ComicInfo.Inst.PanelMode == PanelMode.DoublePage)
                        {
                            bookView.ChangeView(null, bookView.VerticalOffset + 30, null);
                        }
                    }
                }
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Up)
            {
                if (ComicInfo.Inst.Zoom != ZoomType.Fit)
                {
                    if (AppSettings.FlipView)
                    {
                        if (ComicInfo.Inst.PanelMode == PanelMode.SinglePage)
                        {
                        }
                        else if (ComicInfo.Inst.PanelMode == PanelMode.DoublePage)
                        {
                        }
                    }
                    else
                    {
                        e.Handled = true;
                        if (ComicInfo.Inst.PanelMode == PanelMode.SinglePage)
                        {
                            pageView.ChangeView(null, pageView.VerticalOffset - 30, null);
                        }
                        else if (ComicInfo.Inst.PanelMode == PanelMode.DoublePage)
                        {
                            bookView.ChangeView(null, bookView.VerticalOffset - 30, null);
                        }
                    }
                }
            }
        }

        #endregion Grid / Tap / Keydown / Top Level methods

        #region Continuous View methods

        private void continuousView_LayoutUpdated(object sender, object e)
        {
            try
            {
                if (continousViewJumpToPage != null)
                {
                    continuousView.ScrollIntoView(continousViewJumpToPage);
                    continousViewJumpToPage = null;
                }
            }
            catch (Exception ex)
            {
                ShowError("Continuous View Page Navigation", ex);
            }
        }

        private void continuousView_CleanUpVirtualizedItemEvent(object sender, CleanUpVirtualizedItemEventArgs e)
        {
            return;

            //ComicImageViewModel image = e.Value as ComicImageViewModel;

            //if (image != null)
            //{
            //    image.UnsetImageData();
            //}

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

        #endregion Continuous View methods

        #region Book Flip view methods

        private void bookFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                ComicImageViewModel tempImage = item as ComicImageViewModel;

                if (tempImage != null)
                {
                    tempImage.UnsetImageData();
                    tempImage.InvalidateData();
                }
            }
        }

        private void bookFlipView_CleanUpVirtualizedItemEvent(object sender, CleanUpVirtualizedItemEventArgs e)
        {

            ComicImageViewModel image = e.Value as ComicImageViewModel;

            if (image != null)
            {
                image.UnsetImageData();
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

        #endregion Book Flip view methods

        #region Page flip view methods

        private void pageFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                ComicImageViewModel tempImage = item as ComicImageViewModel;

                if (tempImage != null)
                {
                    tempImage.UnsetImageData();
                    tempImage.InvalidateData();
                }
            }
        }

        private void pageFlipView_CleanUpVirtualizedItemEvent(object sender, CleanUpVirtualizedItemEventArgs e)
        {
            ComicImageViewModel image = e.Value as ComicImageViewModel;

            if (image != null)
            {
                image.UnsetImageData();
            }
        }

        #endregion Page flip view methods
    }
}