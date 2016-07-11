using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ComicViewer
{
    public class ComicBookSetting : INotifyPropertyChanged
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

        #endregion

        public ComicBookSetting()
        {
            DefaultValue();
        }

        public ComicBookSetting(ApplicationDataCompositeValue setting)
        {
            try
            {
                UnPackSetting(setting);

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
                ApplicationDataCompositeValue retVal = PackSetting();
                return retVal;
            }
        }

        public int CurrentPage { get; set; }
        public string FileName { get; set; }
        public PanelMode PanelMode{get;set;}
        public ZoomType Zoom{get;set;}
        public RotatePage Rotate { get; set; }
        public DateTime LastRead { get; set; }

        public int PreviousCurrentPage { get; set; }
        public string PreviousFileName { get; set; }
        public PanelMode PreviousPanelMode { get; set; }
        public ZoomType PreviousZoom { get; set; }
        public RotatePage PreviousRotate { get; set; }

        public List<int> Bookmarks { get; set; }

        public void DefaultValue()
        {
            CurrentPage = 1;
            FileName = "";
            Zoom = ZoomType.FitWidth;
            LastRead = DateTime.Now;
            Rotate = RotatePage.RotateNormal;
            PanelMode = ComicViewer.PanelMode.ContniousPage;

            PreviousCurrentPage = 1;
            PreviousFileName = "";
            PreviousPanelMode = ComicViewer.PanelMode.ContniousPage;
            PreviousRotate = RotatePage.RotateNormal;
            PreviousZoom = ZoomType.FitWidth;

            Bookmarks = new List<int>();
        }

        public void UnPackSetting(ApplicationDataCompositeValue setting)
        {
            CurrentPage = (int)setting["CurrentPage"];
            FileName = setting["FileName"] as string;
            Zoom = (ZoomType)setting["Zoom"];
            PanelMode = (PanelMode)setting["PanelMode"];
            Rotate = (RotatePage)setting["Rotate"];
            LastRead = DateTime.Parse(setting["LastRead"] as string);

            string tempBookmarks = setting["Bookmarks"] as string;

            Bookmarks = new List<int>();

            if (!string.IsNullOrWhiteSpace(tempBookmarks))
            {
                Bookmarks = tempBookmarks.ToIntList();
            }
        }

        public ApplicationDataCompositeValue PackSetting()
        {
            ApplicationDataCompositeValue retVal = new ApplicationDataCompositeValue();

            retVal["CurrentPage"] = CurrentPage;
            retVal["FileName"] = FileName;
            retVal["Zoom"] = (int)Zoom;
            retVal["Rotate"] = (int)Rotate;
            retVal["LastRead"] = DateTime.Now.ToString();
            retVal["PanelMode"] = (int)PanelMode;
            retVal["Bookmarks"] = Bookmarks.ToCSString();
            return retVal;
        }
    }
}
