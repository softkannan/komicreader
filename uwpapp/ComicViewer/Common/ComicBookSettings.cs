﻿using ComicViewer.ComicViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ComicViewer.Common
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

        #endregion INotifyPropertyChanged Members

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

        public uint CurrentPage { get; set; }
        public string FileName { get; set; }
        public PanelMode PanelMode { get; set; }
        public ZoomType Zoom { get; set; }
        public RotatePage Rotate { get; set; }
        public DateTime LastRead { get; set; }

        public uint PreviousCurrentPage { get; set; }
        public string PreviousFileName { get; set; }
        public PanelMode PreviousPanelMode { get; set; }
        public ZoomType PreviousZoom { get; set; }
        public RotatePage PreviousRotate { get; set; }

        public List<uint> Bookmarks { get; set; }

        public void DefaultValue()
        {
            CurrentPage = 1;
            FileName = "";
            Zoom = ZoomType.FitWidth;
            LastRead = DateTime.Now;
            Rotate = RotatePage.RotateNormal;
            PanelMode = PanelMode.ContniousPage;

            PreviousCurrentPage = 1;
            PreviousFileName = "";
            PreviousPanelMode = PanelMode.ContniousPage;
            PreviousRotate = RotatePage.RotateNormal;
            PreviousZoom = ZoomType.FitWidth;

            Bookmarks = new List<uint>();
        }

        public void UnPackSetting(ApplicationDataCompositeValue setting)
        {
            CurrentPage = (uint)setting["CurrentPage"];
            FileName = setting["FileName"] as string;
            Zoom = (ZoomType)setting["Zoom"];
            PanelMode = (PanelMode)setting["PanelMode"];
            Rotate = (RotatePage)setting["Rotate"];
            LastRead = DateTime.Parse(setting["LastRead"] as string);

            string tempBookmarks = setting["Bookmarks"] as string;

            Bookmarks = new List<uint>();

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