using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ComicViewer
{
    public interface IArchive
    {
        ArchiveImages Pages{ get; }
        Size PageSize { get; }
        void Close();
    }

    public class ArchiveImages : ObservableCollection<ArchiveImage>
    {
    }

    public class ArchiveImage
    {
        Stream imageData;

        public virtual Stream ImageData
        {
            get { return imageData; }
        }
        int pageNo;

        public virtual int PageNo
        {
            get { return pageNo; }
        }
        
        public ArchiveImage(Stream data, int pageNo)
        {
            this.imageData = data;
            this.pageNo = pageNo;
        }
    }
}
