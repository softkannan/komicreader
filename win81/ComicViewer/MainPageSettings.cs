using SharpCompress.Archive;
using SharpCompress.Archive.Rar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ComicViewer
{
    public partial class MainPage
    {
        ComicBookSetting currentSetting = null;

        public EffectSettings EffectSettings
        {
            get
            {
                App tempApp = Application.Current as App;

                return tempApp.EffectSettings;
            }
        }

        public ComicAppSetting AppSettings
        {
            get {

                App app = Application.Current as App;

                return app.AppSettings;
            
            }
        }

        string FileName
        {
            get
            {
                if (currentSetting != null)
                {
                    return currentSetting.FileName;
                }
                return "";
            }
        }

        public List<int> Bookmarks
        {
            get
            {

                if (currentSetting != null)
                {
                    return currentSetting.Bookmarks;
                }
                return null;
            }
        }

        public int CurrentPage
        {
            get
            {

                if (currentSetting != null)
                {
                    return currentSetting.CurrentPage;
                }
                return 0;
            }
            set
            {
                if (currentSetting != null)
                {
                    currentSetting.PreviousCurrentPage = currentSetting.CurrentPage;
                    currentSetting.CurrentPage = value;
                    InternalPropertyChanged("CurrentPage");
                }
            }
        }

        RotatePage Rotation
        {
            get
            {
                if (currentSetting != null)
                {
                    return currentSetting.Rotate;
                }
                return RotatePage.RotateNormal;
            }
            set
            {
                if (currentSetting != null)
                {
                    currentSetting.PreviousRotate = currentSetting.Rotate;
                    currentSetting.Rotate = value;
                }

            }
        }

        PanelMode PanelMode
        {
            get
            {
                if (currentSetting != null)
                {
                    return currentSetting.PanelMode;
                }
                return ComicViewer.PanelMode.SinglePage;
            }
            set
            {
                if (currentSetting != null)
                {
                    currentSetting.PreviousPanelMode = currentSetting.PanelMode;
                    currentSetting.PanelMode = value;
                }
            }
        }

        ZoomType Zoom
        {
            get
            {
                if (currentSetting != null)
                {
                    return currentSetting.Zoom;
                }
                return ZoomType.Fit;
            }
            set
            {
                if (currentSetting != null)
                {
                    currentSetting.PreviousZoom = currentSetting.Zoom;
                    currentSetting.Zoom = value;
                }

            }
        }

        IArchive comicFileReader = null;

        public ComicImages Pages { get; set; }

        void SaveSettings(string fileName)
        {
            if (currentSetting == null)
            {
                return;
            }
            SettingManager.History.Values[Path.GetFileNameWithoutExtension(fileName)] = currentSetting.Setting;
        }

        void RestoreSettings(string fileName)
        {
            object tempSetting;
            if (SettingManager.History.Values.TryGetValue(Path.GetFileNameWithoutExtension(fileName), out tempSetting))
            {
                currentSetting = new ComicBookSetting(tempSetting as ApplicationDataCompositeValue);
                currentSetting.FileName = fileName;
            }
            else
            {
                currentSetting = new ComicBookSetting();
                currentSetting.FileName = fileName;
                currentSetting.PanelMode = AppSettings.PanelMode;
            }
        }

    }

    public class EffectSetting
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasValue { get; set; }
        public double Value { set; get; }
        public ImageEffect Type { get; set; }
    }

    public class EffectSettings : List<EffectSetting>
    {
        public Func<Task> EffectChanged = null;
    }
}
