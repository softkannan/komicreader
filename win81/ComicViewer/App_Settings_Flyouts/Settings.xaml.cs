using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ComicViewer
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Settings : LayoutAwarePage
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        ComicAppSetting appSettings = null;

        public ComicAppSetting AppSettings
        {
            get { return appSettings; }
            set {

                appSettings = value;
                UpdateControls();
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }


        private void CloseFlyout(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Popup)
                (this.Parent as Popup).IsOpen = false;

            SettingsPane.Show();
        }

        bool IgnoreEvent = false;

        private void UpdateControls()
        {
            if (AppSettings == null)
            {
                return;
            }

            IgnoreEvent = true;

            chkRightToLeft.IsChecked = AppSettings.RightToLeft;
            txtZoomStep.Text = AppSettings.ZoomStep.ToString();
            //comboAutoRotation.SelectedIndex = (int)AppSettings.AutoRotation;
            //txtCachePages.Text = AppSettings.CachePages.ToString();
            chkFlipView.IsChecked = AppSettings.FlipView;
            comboView.SelectedIndex = (int)AppSettings.PanelMode;
            comboMouseFlipType.SelectedIndex = (int)AppSettings.MouseFlipType;

            IgnoreEvent = false;
        }

        private void bttnResetAll_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings == null)
            {
                return;
            }

            AppSettings.DefaultValue();

            UpdateControls();
        }

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

        private async void ShowMessage(string message)
        {
            MessageDialog messageDia = new MessageDialog(message, "Info");
            await messageDia.ShowAsync();
        }

        private async void ShowError(string title, string message)
        {
            MessageDialog messageDia = new MessageDialog(string.Format("{0}{1}", title, message), "Error");
            await messageDia.ShowAsync();
        }

        private async void bttnApply_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings != null)
            {
                bool isFlipView = chkFlipView.IsChecked ?? true;

               bool isRightToLeft = chkRightToLeft.IsChecked ?? false;

                if (!txtZoomStep.ValidateMinMax(0.01, 0.5))
                {
                    ShowError("Error", "Invalid Zoom Step");
                    return;
                }

               float tempZoomStep = txtZoomStep.Text.ParseText(0.1f);

               // AppSettings.AutoRotation = (AutoRotationPreference)comboAutoRotation.SelectedIndex;

               AppSettings.PanelMode = (PanelMode) comboView.SelectedIndex;

                //AppSettings.CachePages = ParseValue(txtCachePages.Text, 5);

               AppSettings.MouseFlipType = (MousePageFlipType)comboMouseFlipType.SelectedIndex;


               AppSettings.RightToLeft = isRightToLeft;
               AppSettings.ZoomStep = tempZoomStep;
               AppSettings.FlipView = isFlipView;

               AppSettings.SaveSettings();

               if (AppSettings.AppSettingsChanged != null)
               {
                   await AppSettings.AppSettingsChanged();

                   ShowMessage("Successfully Settings Applied.");
               }
            }
        }

        private void txtZoomStep_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreEvent)
            {
                return;
            }

            txtZoomStep.ValidateMinMax(0.01, 0.5);
        }

        

       

        //private void txtCachePages_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (IgnoreEvent)
        //    {
        //        return;
        //    }

        //    ValidateMinMax(txtZoomStep, 0, 15);
        //}
    }
}
