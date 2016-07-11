using SharpCompress.Archive;
using SharpCompress.Archive.Rar;
using Softbuild.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicViewer
{
    public class ComicImageViewModelList : List<ComicImageViewModel>
    {
        //public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
        //public ScrollMode HorizontalScrollMode { get; set; }
        //public ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
        //public ScrollMode VerticalScrollMode { get; set; }
        //public ZoomMode ZoomMode { get; set; }
        //public float ZoomFactor { get; set; }
    }
}