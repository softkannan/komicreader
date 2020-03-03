using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;

namespace ComicViewer
{
    public class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager mgr = new SingleInstanceManager();
            mgr.Run(args);
        }
    }

    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        App app;

        public SingleInstanceManager()
        {
            this.IsSingleInstance = true;
        }
        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            app = new App();
            app.InitializeComponent();
            app.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            app.NextInstance(eventArgs);
        }

    }


    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public event EventHandler<StartupNextInstanceEventArgs> StartupNextInstance;

        public void NextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            if (StartupNextInstance != null)
            {
                StartupNextInstance(this,eventArgs);
            }
            this.MainWindow.Activate();
        }
    }
}
