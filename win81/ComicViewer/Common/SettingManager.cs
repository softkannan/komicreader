using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ComicViewer
{
    public class SettingManager
    {
        private SettingManager()
        {

        }
        public static ApplicationDataContainer Inst
        {
            get
            {
                return ApplicationData.Current.LocalSettings; ;
            }
        }
        public static ApplicationDataContainer History
        {
            get
            {
                return ApplicationData.Current.LocalSettings.CreateContainer("History", ApplicationDataCreateDisposition.Always);
            }
        }
        public static ApplicationDataContainer Settings
        {
            get
            {
                return ApplicationData.Current.LocalSettings.CreateContainer("Settings", ApplicationDataCreateDisposition.Always);
            }
        }

        public static void ClearHistory()
        {
            var count = History.Values.Count;

            if (count > 100)
            {

            }

        }
    }
}
