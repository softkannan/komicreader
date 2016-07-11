using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.ComponentModel;
using Windows.UI.Xaml;
using SharpCompress.Archive.Rar;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Core;
using Softbuild.Media;
using Windows.UI.Xaml.Controls;
using System.Collections;
using System.Threading;
using Windows.UI.Popups;

namespace ComicViewer
{
    public class ComicImages : List<ComicImage>
    {
        //public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
        //public ScrollMode HorizontalScrollMode { get; set; }
        //public ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
        //public ScrollMode VerticalScrollMode { get; set; }
        //public ZoomMode ZoomMode { get; set; }
        //public float ZoomFactor { get; set; }
    }
  
}
