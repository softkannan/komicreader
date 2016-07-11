using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
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
                gotoPage.SetPages(Pages, Bookmarks, CurrentPage);
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
                    if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
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
                    navSync.Release();
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

        #endregion Rotate and Zoom methods

        #region Grid / Tap / Keydown / Top Level methods

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
                    if (navSync.Wait(NAVIGATION_OPERATION_WAIT_TIME))
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
                    navSync.Release();
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

        private void TopAppBar_Opened(object sender, object e)
        {
            UpdateCurrentPage();

            if (!string.IsNullOrWhiteSpace(FileName))
            {
                SaveSettings(FileName);
            }
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
                            pageView.ChangeView(null, pageView.VerticalOffset + 30, null);
                        }
                        else if (PanelMode == ComicViewer.PanelMode.DoublePage)
                        {
                            bookView.ChangeView(null, bookView.VerticalOffset + 30, null);
                        }
                    }
                }
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Up)
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
                            pageView.ChangeView(null, pageView.VerticalOffset - 30, null);
                        }
                        else if (PanelMode == ComicViewer.PanelMode.DoublePage)
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
            ComicImageViewModel image = e.Value as ComicImageViewModel;

            if (image != null)
            {
                image.UnsetImageData();
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

        #endregion Continuous View methods

        #region Book Flip view methods

        private async void bookFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                ComicImageViewModel tempImage = item as ComicImageViewModel;

                if (tempImage != null)
                {
                    tempImage.UnsetImageData();
                    await tempImage.InvalidateData();
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

        private async void pageFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                ComicImageViewModel tempImage = item as ComicImageViewModel;

                if (tempImage != null)
                {
                    tempImage.UnsetImageData();
                    await tempImage.InvalidateData();
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