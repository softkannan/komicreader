using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace ComicViewer
{
    public partial class MainPage
    {
        private void bttnSettings_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ShowFlyoutDialogs("SE");
        }
        private void bttnEffects_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ShowFlyoutDialogs("EF");
        }
        private void bttnTermsandConditions_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ShowFlyoutDialogs("TC");
        }
        private void bttnContactUs_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ShowFlyoutDialogs("CU");
        }
        private void bttnAboutUs_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ShowFlyoutDialogs("AU");
        }

        private async void ShowFlyoutDialogs(string command)
        {
            ContentDialog settingsDialog = new ContentDialog();
            UserControl settingPage = null;

            switch (command)
            {
                case "AU":
                    settingPage = new AboutUs();
                    settingsDialog.Title = "About Us";
                    settingsDialog.PrimaryButtonText = "Ok";
                    break;

                case "CU":
                    settingPage = new ContactUs();
                    settingsDialog.Title = "Contact Us";
                    settingsDialog.PrimaryButtonText = "Ok";
                    break;

                case "SE":
                    {
                        var tempSettings = new Settings();
                        tempSettings.AppSettings = AppSettings;
                        settingPage = tempSettings;
                    }
                    settingsDialog.Title = "Settings";
                    settingsDialog.PrimaryButtonText = "Ok";
                    break;

                case "TC":
                    settingPage = new TermsAndConditions();
                    settingsDialog.Title = "Terms and Conditions";
                    settingsDialog.PrimaryButtonText = "Ok";
                    break;

                case "EF":
                    {
                        var tempEffPage = new EffectsPage();
                        settingPage = tempEffPage;
                        tempEffPage.Settings = EffectSettings;
                        settingsDialog.Title = "Effects [Exprimental]";
                        settingsDialog.PrimaryButtonText = "Ok";
                    }
                    break;
            }

            // Place the SettingsFlyout inside our Popup window.
            settingsDialog.Content = settingPage;
            ContentDialogResult result = await settingsDialog.ShowAsync();
        }
    }
}
