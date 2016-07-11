using ComicViewer.ComicModel;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

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

            //effects setting changed event handler.
            //force reload the image data from source and apply the effects
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

            //setting changed event handler.
            //force reload the image data from source and apply the effects

            AppSettings.AppSettingsChanged = async () =>
            {
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
            ComicImageViewModel.PageSize = availableSize;

            PageViewHeight = availableSize.Height;
            PageViewWidth = availableSize.Width;

            var asyncTask = ComicImage.SetDefaultImageAsync(availableSize);

            asyncTask.Wait();

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

        private async void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                await ShowPage();
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