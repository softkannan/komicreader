using ComicViewer.ComicModel;
using ComicViewer.ComicViewModel;
using ComicViewer.Common;
using SharpCompress.Archives;
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
        //private ComicBookSetting currentSetting = null;

        public EffectSettings EffectSettings { get => ComicInfo.Inst.EffectSettings; }

        public ComicAppSetting AppSettings { get => ComicInfo.Inst.AppSettings; }

        public List<uint> Bookmarks { get => ComicInfo.Inst.Bookmarks; }

        public float ZoomFactor { get => ComicInfo.Inst.ZoomFactor; }

        public uint CurrentPage { get => ComicInfo.Inst.CurrentPage; }

        public RotatePage Rotation { get => ComicInfo.Inst.Rotation; }

        public PanelMode PanelMode { get => ComicInfo.Inst.PanelMode; }

        public ZoomType Zoom { get => ComicInfo.Inst.Zoom; }
     
        private IDocument _documentReader = null;

        public ComicImageViewModelList Pages { get; set; }
    }

 


}