using ComicViewer.ComicModel;
using ComicViewer.ComicViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ComicViewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged, IDisposable
    {
        //contains the command line options supplied by the Windows Shell
        private object userArgs = null;

        private bool disposed = false;

        public MainPage()
        {
            this.InitializeComponent();

            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(titleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;


            ComicInfo.Inst.Initialize(InternalPropertyChanged, UpdateCurrentPage);

            //effects setting changed event handler.
            //force reload the image data from source and apply the effects
            EffectSettings.EffectChanged = () =>
            {
                try
                {
                    Busy();
                    ShowPage();
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

            //setting changed event handler.
            //force reload the image data from source and apply the effects

            AppSettings.AppSettingsChanged = () =>
            {
                try
                {
                    Busy();
                    ShowPage();
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

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

            continuousView.LayoutUpdated += continuousView_LayoutUpdated;

            this.DataContext = this;
        }

        

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
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

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
                var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                var size = new Size( (bounds.Width + bounds.Left) * scaleFactor, (bounds.Height + bounds.Top) * scaleFactor);
                PDFPage.SetDefaultPageSize(size);
                Exception tempEx = null;
                try
                {
                    if (userArgs != null)
                    {
                        CloseComic();

                        Busy();

                        //TODO: File navigation needs to be fixed
                        if (userArgs is NavigationEventArgs)
                        {
                            userArgs = null;
                            await OpenComicAsync();
                        }
                        else if(userArgs is FileActivatedEventArgs)
                        {
                            var args = userArgs as FileActivatedEventArgs;
                            userArgs = null;
                            if (args != null)
                            {
                                await OpenComicAsync(args.Files.FirstOrDefault());
                            }
                        }
                        else if (userArgs is LaunchActivatedEventArgs)
                        {
                            var args = userArgs as LaunchActivatedEventArgs;
                            userArgs = null;
                            if (args != null)
                            {
                                if (string.IsNullOrWhiteSpace(args.Arguments))
                                {
                                    await OpenComicAsync();
                                }
                                else
                                {
                                    StorageFile openFile = await StorageFile.GetFileFromPathAsync(args.Arguments);
                                    await OpenComicAsync(openFile);
                                }
                            }
                        }
                    }
                    else
                    {
                        Busy();
                        ShowPage();
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

            //continuousView.Width = e.Size.Width;
            //continuousView.Height = e.Size.Height;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            ComicImageViewModel.PageSize = availableSize;

            ComicInfo.Inst.PageViewHeight = availableSize.Height;
            ComicInfo.Inst.PageViewWidth = availableSize.Width;
            ComicInfo.Inst.InvalidatePageSize();

            //ComicImage.CreateDefaultImage(availableSize);

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

        #endregion INotifyPropertyChanged Members

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                ShowPage();
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        // Protected implementation of Dispose pattern.
        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                if (navSync != null)
                {
                    navSync.Dispose();
                }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        
        
    }
}
