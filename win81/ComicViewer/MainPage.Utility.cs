using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ComicViewer
{
    public partial class MainPage
    {
        private async void ShowError(string title, Exception ex)
        {
            MessageDialog message = new MessageDialog(string.Format("{0} error : {1}", title, ex.Message), "Error");
            await message.ShowAsync();
        }

        private async Task ShowErrorAsync(string title, Exception ex)
        {
            MessageDialog message = new MessageDialog(string.Format("{0} error : {1}", title, ex.Message), "Error");
            await message.ShowAsync();
        }

        private async void ShowError(string title, string message)
        {
            MessageDialog messageDia = new MessageDialog(string.Format("{0}{1}", title, message), "Error");
            await messageDia.ShowAsync();
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
