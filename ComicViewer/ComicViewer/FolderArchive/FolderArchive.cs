using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace ComicViewer
{
    class FolderArchive : IArchive
    {
        Dictionary<string, Stream> fileCache = new Dictionary<string, Stream>();
        ArchiveImages images = new ArchiveImages();
        Size pageSize = new Size(1, 1);
        SortedDictionary<string, string> fileList = new SortedDictionary<string, string>();

        public FolderArchive(string fileName)
        {
            foreach (string item in Directory.GetFiles(fileName,"*.*", SearchOption.TopDirectoryOnly))
            {
                string tempExtn = Path.GetExtension(item).ToLower();
                if (ImagefileUtility.IsImageFile(tempExtn))
                {
                    fileList.Add(Path.GetFileName(item), item);
                }
            }
            int index = 0;
            foreach (string item in fileList.Keys)
            {
                FolderImage tempImg = new FolderImage(this, item, index + 1);
                images.Add(tempImg);
                if (index == 0)
                {
                    using (System.Drawing.Bitmap bitImg = new System.Drawing.Bitmap(tempImg.ImageData))
                    {
                        pageSize = new Size(bitImg.Width, bitImg.Height);
                    }
                }
                index++;
            }

        }
               
        public Stream this[string fileName]
        {
            get
            {
                Stream retVal = null;

                if (fileCache.ContainsKey(fileName))
                {
                    retVal = fileCache[fileName];
                    retVal.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    retVal = File.Open(fileList[fileName], FileMode.Open, FileAccess.Read, FileShare.Read);
                    fileCache.Add(fileName, retVal);
                }
                return retVal;
            }
        }

        #region IArchive Members

        public ArchiveImages Pages
        {
            get { return images; }
        }

        public System.Windows.Size PageSize
        {
            get { return pageSize; }
        }

        public void Close()
        {
            foreach (IDisposable item in fileCache.Values)
            {
                item.Dispose();
            }
            fileList.Clear();
        }

        #endregion
    }

    class FolderImage : ArchiveImage
    {
        FolderArchive folderFile;
        string imgFileName;

        public FolderImage(FolderArchive folderFile, string imgFileName, int pageNo)
            : base(null, pageNo)
        {
            this.folderFile = folderFile;
            this.imgFileName = imgFileName;
        }
        public override Stream ImageData
        {
            get
            {
                return this.folderFile[this.imgFileName];
            }
        }
    }
}
