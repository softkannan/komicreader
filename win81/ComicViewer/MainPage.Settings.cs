﻿using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace ComicViewer
{
    public partial class MainPage
    {
        private ComicBookSetting currentSetting = null;

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
            get
            {
                App app = Application.Current as App;

                return app.AppSettings;
            }
        }

        private string FileName
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

        private RotatePage Rotation
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

        private PanelMode PanelMode
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

        private ZoomType Zoom
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

        private IArchive comicFileReader = null;

        public ComicImageViewModelList Pages { get; set; }

        private void SaveSettings(string fileName)
        {
            if (currentSetting == null)
            {
                return;
            }
            SettingManager.History.Values[Path.GetFileNameWithoutExtension(fileName)] = currentSetting.Setting;
        }

        private void RestoreSettings(string fileName)
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