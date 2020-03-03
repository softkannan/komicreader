using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows;

namespace ComicViewer
{
    class TestWindow : Window
    {
        Image imgCtrl = new Image();
        public void AddBitmap(BitmapImage img)
        {
             //Grid testGrid = new Grid();
                    
                    imgCtrl.Source = img;

                    //testGrid.Children.Add(imgCtrl);

                    this.AddChild(imgCtrl);
        }
    }
    class ZipArchive : IArchive
    {
        
        ZipFile file = null;
        Size pageSize = new Size(1, 1);

        public Size PageSize
        {
            get { return pageSize; }
        }

        ArchiveImages images = new ArchiveImages();

        #region IArchive Members

        public ZipArchive(string fileInfo)
        {
            List<string> fileList = new List<string>();
            file = new ZipFile(fileInfo);
                       
            foreach (ZipEntry item in file)
            {
                if (item.IsFile)
                {
                    string tempExtn = Path.GetExtension(item.Name).ToLower();
                    if (ImagefileUtility.IsImageFile(tempExtn))
                    {
                        fileList.Add(item.Name.ToLower());
                    }
                }
            }

            fileList.Sort();

            for (int index = 0; index < fileList.Count; index++)
            {
                ZipEntry tempEntry = file.GetEntry(fileList[index]);
                ArchiveImage tempImg = new ZipArchiveImage(file,tempEntry, index + 1);
                images.Add(tempImg);
                if (index == 0)
                {
                    using (Stream data = tempImg.ImageData)
                    {
                        using (System.Drawing.Bitmap bitImg = new System.Drawing.Bitmap(data))
                        {
                            pageSize = new Size(bitImg.Width, bitImg.Height);
                        }
                    }


                }
            }
        }
        
        public ArchiveImages Pages
        {
            get
            {
                return images;
            }
        }
        
       
        public void Close()
        {
            if (images != null)
            {
                images.Clear();
                images = null;
            }
            if (file != null)
            {
                file.Close();
                file = null;
            }
        }
        #endregion
    }

    public class ZipArchiveImage : ArchiveImage
    {
        ZipEntry entry;
        ZipFile file;
        public ZipArchiveImage(ZipFile file, ZipEntry entry, int pageNo)
            : base(null, pageNo)
        {
            this.file = file;
            this.entry = entry;
        }
        public override Stream ImageData
        {
            get
            {
                return file.GetInputStream(entry);
            }
        }
    }
}
