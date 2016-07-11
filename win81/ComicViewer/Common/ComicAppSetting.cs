using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ComicViewer
{
    public class ComicAppSetting : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void InternalPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged Members

        public Func<Task> AppSettingsChanged { get; set; }

        public ComicAppSetting()
        {
            AppSettingsChanged = null;

            DefaultValue();
        }

        public ComicAppSetting(ApplicationDataCompositeValue setting)
        {
            try
            {
                UnPackSetting(setting);
                AppSettingsChanged = null;
            }
            catch (Exception)
            {
                DefaultValue();
            }
        }

        public ApplicationDataCompositeValue Setting
        {
            get
            {
                return PackSetting();
            }
        }

        public void SaveSettings()
        {
            SettingManager.Settings.Values["Settings"] = this.Setting;
        }

        public bool RightToLeft { get; set; }
        public float ZoomMin { get; set; }
        public float ZoomMax { get; set; }
        public float ZoomStep { get; set; }
        public AutoRotationPreference AutoRotation { get; set; }
        public int CachePages { get; set; }
        public bool FlipView { get; set; }
        public PanelMode PanelMode { get; set; }
        public MousePageFlipType MouseFlipType { get; set; }

        public void DefaultValue()
        {
            RightToLeft = false;
            ZoomMax = 1.0f;
            ZoomMin = 0.1f;
            ZoomStep = 0.1f;
            AutoRotation = AutoRotationPreference.On;
            CachePages = 5;
            FlipView = false;
            PanelMode = ComicViewer.PanelMode.ContniousPage;
            MouseFlipType = MousePageFlipType.Double;
        }

        public void UnPackSetting(ApplicationDataCompositeValue setting)
        {
            RightToLeft = (bool)setting["RightToLeft"];

            ZoomMax = (float)setting["ZoomMax"];
            ZoomMin = (float)setting["ZoomMin"];
            ZoomStep = (float)setting["ZoomStep"];
            AutoRotation = (AutoRotationPreference)setting["AutoRotation"];
            CachePages = (int)setting["CachePages"];
            FlipView = (bool)setting["FlipView"];
            PanelMode = (PanelMode)setting["PanelMode"];
            MouseFlipType = (MousePageFlipType)setting["MouseFlipType"];
        }

        public ApplicationDataCompositeValue PackSetting()
        {
            ApplicationDataCompositeValue retVal = new ApplicationDataCompositeValue();

            retVal["RightToLeft"] = RightToLeft;
            retVal["ZoomMax"] = ZoomMax;
            retVal["ZoomMin"] = ZoomMin;
            retVal["ZoomStep"] = ZoomStep;
            retVal["AutoRotation"] = (int)AutoRotation;
            retVal["CachePages"] = CachePages;
            retVal["FlipView"] = FlipView;
            retVal["PanelMode"] = (int)PanelMode;
            retVal["MouseFlipType"] = (int)MouseFlipType;

            return retVal;
        }
    }
}