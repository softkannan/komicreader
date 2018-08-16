using ComicViewer.ComicViewModel;
using ComicViewer.Common;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ComicViewer
{
    public class ComicInfo
    {
        private static ComicInfo _comicInfo = new ComicInfo();
        public static ComicInfo Inst { get => _comicInfo; }

        private ComicBookSetting currentSetting = null;

        public Action<string> _propertyChanged = null;

        public Action _updateUICurrentPage = null;

        public volatile bool IsFullScreen = false;

        public void Initialize(Action<string> propertyChanged, Action updateUICurrentPage)
        {
            _propertyChanged = propertyChanged;
            _updateUICurrentPage = updateUICurrentPage;
        }

        public void NotifyPageChange()
        {
            if (!CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                //UpdateUICurrentPage();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                         () =>
                         {
                                 // update your UI here
                                 _updateUICurrentPage?.Invoke();

                         });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                _updateUICurrentPage?.Invoke();
            }
        }

        private void InternalPropertyChanged(string propertyName)
        {
            if (!CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                             () =>
                             {
                                 // update your UI here
                                 _propertyChanged?.Invoke(propertyName);
                             });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                _propertyChanged?.Invoke(propertyName);
            }
        }

        public EffectSettings EffectSettings
        {
            get
            {
                App tempApp = Application.Current as App;

                return tempApp.EffectSettings;
            }
        }

        public float ZoomFactor { get; set; }

        public ComicAppSetting AppSettings
        {
            get
            {
                App app = Application.Current as App;

                return app.AppSettings;
            }
        }

        public string ComicFileName
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

        public List<uint> Bookmarks
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

        public uint CurrentPage
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

        private uint totalPage = 0;
        private double pageViewHeight = 0;
        private double pageViewWidth = 0;

        public uint LastPage
        {
            get
            {
                return totalPage;
            }
            set
            {
                totalPage = value;
                InternalPropertyChanged("LastPage");
            }
        }

        public double PageViewHeight
        {
            get
            {
                return pageViewHeight;
            }
            set
            {
                pageViewHeight = value;
                
            }
        }

        public double PageViewWidth
        {
            get
            {
                return pageViewWidth;
            }
            set
            {
                pageViewWidth = value;
                
            }
        }

        public void InvalidatePageSize()
        {
            InternalPropertyChanged("PageViewHeight");
            InternalPropertyChanged("PageViewWidth");
        }

        public RotatePage Rotation
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

        public PanelMode PanelMode
        {
            get
            {
                if (currentSetting != null)
                {
                    return currentSetting.PanelMode;
                }
                return PanelMode.SinglePage;
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

        public ZoomType Zoom
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

        public void SaveSettings()
        {
            if (currentSetting != null && !string.IsNullOrWhiteSpace(currentSetting.FileName))
            {
                SettingManager.History.Values[Path.GetFileNameWithoutExtension(currentSetting.FileName)] = currentSetting.Setting;
            }
        }

        public void RestoreSettings(string fileName)
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
}
