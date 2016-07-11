using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
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
    public sealed partial class EffectsPage : LayoutAwarePage
    {
        public EffectsPage()
        {
            this.InitializeComponent();
        }

        EffectSettings localSettings = null;

        public EffectSettings Settings
        {
            get { return localSettings; }
            set {

                localSettings = value;
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
            IgnoreEvent = true;
            foreach (var item in Settings)
            {
                switch (item.Type)
                {
                    case ImageEffect.Grey:
                        chkGrey.IsChecked = item.IsEnabled;
                        break;
                    case ImageEffect.AutoColoring:
                        chkAutoColoring.IsChecked = item.IsEnabled;
                        break;
                    case ImageEffect.Bakumatsu:
                        chkBakumatsu.IsChecked = item.IsEnabled;
                        break;
                    case ImageEffect.Contrast:
                        chkContrast.IsChecked = item.IsEnabled;
                        valueContrast.Value = item.Value;
                        break;
                    case ImageEffect.Posterize:
                        chkPosterize.IsChecked = item.IsEnabled;
                        valuePosterize.Value = item.Value;
                        break;
                }
            }
            IgnoreEvent = false;
        }

        private void Effects_Checked(object sender, RoutedEventArgs e)
        {
            if (IgnoreEvent)
            {
                return;
            }
            CheckBox tempBox = sender as CheckBox;

            UpdateCheckBox(tempBox);
        }

        private void UpdateCheckBox(CheckBox tempBox)
        {
            if (tempBox != null)
            {
                var foundEffect = Settings.First((item) => item.Name == tempBox.Tag as string);

                if (foundEffect != null)
                {
                    foundEffect.IsEnabled = tempBox.IsChecked ?? false;
                }
            }
        }

        private void bttnClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Settings)
            {
                item.IsEnabled = false;
            }

            UpdateControls();
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (IgnoreEvent)
            {
                return;
            }
            Slider tempSlider = sender as Slider;

            if (tempSlider != null)
            {
                var foundEffect = Settings.First((item) => item.Name == tempSlider.Tag as string);

                if (foundEffect != null)
                {
                    foundEffect.Value = tempSlider.Value;
                }
            }
        }

        private void Effects_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IgnoreEvent)
            {
                return;
            }
            CheckBox tempBox = sender as CheckBox;

            UpdateCheckBox(tempBox);
        }

        private async void bttnApply_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.EffectChanged != null)
            {
                await Settings.EffectChanged();
            }
        }
    }
}
