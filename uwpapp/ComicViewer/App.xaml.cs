using ComicViewer.ComicViewModel;
using ComicViewer.Common;
using ComicViewer.Control;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ComicViewer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            EffectSettings = new EffectSettings();

            EffectSettings.Add(new EffectSetting() { Name = "Grey", HasValue = false, IsEnabled = false, Type = ImageEffect.Grey });
            EffectSettings.Add(new EffectSetting() { Name = "Invert", HasValue = false, IsEnabled = false, Type = ImageEffect.Invert });
            EffectSettings.Add(new EffectSetting() { Name = "Flip", HasValue = false, IsEnabled = false, Value = (double) WriteableBitmapExtensions.FlipMode.Horizontal, Type = ImageEffect.Flip });
            EffectSettings.Add(new EffectSetting() { Name = "Contrast", HasValue = true, IsEnabled = false, Value = 0.5, Type = ImageEffect.Contrast });
            EffectSettings.Add(new EffectSetting() { Name = "Brightness", HasValue = true, IsEnabled = false, Value = 0, Type = ImageEffect.Brightness });
            EffectSettings.Add(new EffectSetting() { Name = "Convolution", HasValue = true, IsEnabled = false, Value = (double) ImageKernel.Blur, Type = ImageEffect.Convolute });

            RestoreSettings();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            switch (AppSettings.AutoRotation)
            {
                case AutoRotationPreference.Off:
                    DisplayInformation.AutoRotationPreferences = DisplayInformation.GetForCurrentView().CurrentOrientation;
                    break;
                case AutoRotationPreference.PreferLandScape:
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape | DisplayOrientations.LandscapeFlipped;
                    break;
                case AutoRotationPreference.PreferPortrait:
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.PortraitFlipped;
                    break;
            }

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                    rootFrame.Navigate(typeof(MainPage), e);
                    // Ensure the current window is active
                    Window.Current.Activate();
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        async private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        public EffectSettings EffectSettings { get; private set; }

        private ComicAppSetting appSettings = null;

        public ComicAppSetting AppSettings
        {
            get { return appSettings; }
        }

        private void RestoreSettings()
        {
            object tempSetting;
            if (SettingManager.Settings.Values.TryGetValue("Settings", out tempSetting))
            {
                appSettings = new ComicAppSetting(tempSetting as ApplicationDataCompositeValue);
            }
            else
            {
                appSettings = new ComicAppSetting();
            }
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            base.OnFileActivated(args);

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                    rootFrame.Navigate(typeof(MainPage), args);
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args))
                {
                    rootFrame.Navigate(typeof(MainPage), args);
                }
            }
            else
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args))
                {
                    throw new ComicViewException("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }
    }
}
