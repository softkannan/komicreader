using ComicViewer.ComicViewModel;
using ComicViewer.Common;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ComicViewer
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class EffectsPage : UserControl
    {
        public EffectsPage()
        {
            this.InitializeComponent();
        }

        private EffectSettings localSettings = null;

        public EffectSettings Settings
        {
            get { return localSettings; }
            set
            {
                localSettings = value;
                UpdateControls();
            }
        }

        private bool IgnoreEvent = false;

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

                    case ImageEffect.Invert:
                        chkInvert.IsChecked = item.IsEnabled;
                        break;

                    case ImageEffect.Flip:
                        chkFlip.IsChecked = item.IsEnabled;
                        valueFlip.SelectedIndex = (int) item.Value;
                        break;

                    case ImageEffect.Contrast:
                        chkContrast.IsChecked = item.IsEnabled;
                        valueContrast.Value = item.Value;
                        break;

                    case ImageEffect.Brightness:
                        chkBrightness.IsChecked = item.IsEnabled;
                        valueBrightness.Value = item.Value;
                        break;
                    case ImageEffect.Convolute:
                        chkConvolution.IsChecked = item.IsEnabled;
                        valueConvolution.SelectedIndex = (int) item.Value;
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

        private void bttnApply_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.EffectChanged != null)
            {
                Settings.EffectChanged();
            }
        }

        private void valueFlip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreEvent)
            {
                return;
            }
            ComboBox tempCombo = sender as ComboBox;

            if (tempCombo != null)
            {
                var foundEffect = Settings.First((item) => item.Name == tempCombo.Tag as string);

                if (foundEffect != null)
                {
                    switch(valueFlip.SelectedIndex)
                    {
                        default:
                        case 1:
                            foundEffect.Value = (double)WriteableBitmapExtensions.FlipMode.Horizontal;
                            break;
                        case 0:
                            foundEffect.Value = (double)WriteableBitmapExtensions.FlipMode.Vertical;
                            break;
                    }
                }
            }
        }

        private void valueConvolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreEvent)
            {
                return;
            }
            ComboBox tempCombo = sender as ComboBox;

            if (tempCombo != null)
            {
                var foundEffect = Settings.First((item) => item.Name == tempCombo.Tag as string);

                if (foundEffect != null)
                {
                    switch (valueConvolution.SelectedIndex)
                    {
                        default:
                        case 0:
                            foundEffect.Value = (double)ImageKernel.Blur;
                            break;
                        case 1:
                            foundEffect.Value = (double)ImageKernel.EdgeDetect;
                            break;
                        case 2:
                            foundEffect.Value = (double)ImageKernel.Emboss;
                            break;
                        case 3:
                            foundEffect.Value = (double)ImageKernel.Gradient;
                            break;
                        case 4:
                            foundEffect.Value = (double)ImageKernel.Sharpen;
                            break;
                    }
                }
            }
        }
    }
}