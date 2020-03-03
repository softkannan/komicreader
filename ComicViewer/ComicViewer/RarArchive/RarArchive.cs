using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace ComicViewer
{
    class RarArchive:IArchive
    {
        Dictionary<string,Stream> fileCache = new Dictionary<string,Stream>();
        ArchiveImages images = new ArchiveImages();
        Size pageSize = new Size(1, 1);
        string tempExtractFolder;
        string rarFileName;

        internal string RarFileName
        {
            get { return rarFileName; }
        }
        internal string TempExtractFolder
        {
            get { return tempExtractFolder; }
        }
        public RarArchive(string fileName)
        {
            Unrar rarFile = new Unrar();
            SortedList<string, string> tempList = new SortedList<string, string>();

            this.tempExtractFolder = Path.GetTempPath() + "ComicRar\\";

            if (Directory.Exists(this.tempExtractFolder))
            {
                DeleteAll(this.tempExtractFolder);
            }
            if(!Directory.Exists(this.tempExtractFolder))
            {
                Directory.CreateDirectory(this.tempExtractFolder);
            }

            rarFile.Open(fileName, Unrar.OpenMode.List);
            while (rarFile.ReadHeader())
            {
                if (!rarFile.CurrentFile.IsDirectory)
                {
                    string tempExtn = Path.GetExtension(rarFile.CurrentFile.FileName).ToLower();
                    if (ImagefileUtility.IsImageFile(tempExtn))
                    {
                        tempList.Add(Path.GetFileName(rarFile.CurrentFile.FileName), rarFile.CurrentFile.FileName);
                    }
                }
                rarFile.Skip();
            }
            rarFile.Close();
            this.rarFileName = fileName;

            int index = 0;
            foreach (string item in tempList.Keys)
            {
                RarImage tempImg = new RarImage(this, tempList[item], index + 1);
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

        private void DeleteAll(string folderName)
        {
            foreach (string item in Directory.GetDirectories(folderName))
            {
                DeleteAll(item);
            }
            foreach (string item in Directory.GetFiles(folderName, "*.*"))
            {
                File.Delete(item);
            }
            Directory.Delete(folderName);
        }

        private void AttachHandlers(Unrar unrar)
        {
            unrar.ExtractionProgress += new ExtractionProgressHandler(unrar_ExtractionProgress);
            unrar.MissingVolume += new MissingVolumeHandler(unrar_MissingVolume);
            unrar.PasswordRequired += new PasswordRequiredHandler(unrar_PasswordRequired);
        }

        internal static void unrar_ExtractionProgress(object sender, ExtractionProgressEventArgs e)
        {
            //statusBar.Text = "Testing " + e.FileName;
            //progressBar.Value = (int)e.PercentComplete;
        }

        internal static void unrar_MissingVolume(object sender, MissingVolumeEventArgs e)
        {
            //TextInputDialog dialog = new TextInputDialog();
            //dialog.Value = e.VolumeName;
            //dialog.Prompt = string.Format("Volume is missing.  Correct or cancel.");
            //if (dialog.ShowDialog() == DialogResult.OK)
            //{
            //    e.VolumeName = dialog.Value;
            //    e.ContinueOperation = true;
            //}
            //else
            //    e.ContinueOperation = false;
        }
        internal static void unrar_PasswordRequired(object sender, PasswordRequiredEventArgs e)
        {
            TextInputDialog dialog = new TextInputDialog();
            if (dialog.ShowDialog() == true)
            {
                e.Password = dialog.PassWord;
                e.ContinueOperation = true;
            }
            else
                e.ContinueOperation = false;
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
                    string actualFile = this.TempExtractFolder + fileName;
                    if (!File.Exists(actualFile))
                    {
                        try
                        {
                            this.Extract(fileName);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Unable to extract the image", "Error");
                        }
                    }
                    retVal = File.Open(actualFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    fileCache.Add(fileName, retVal);
                }
                return retVal;
            }
        }
        public void Extract(string fileName)
        {
            Unrar rarFileLocal = new Unrar();
            rarFileLocal.Open(this.RarFileName, Unrar.OpenMode.Extract);

            rarFileLocal.MissingVolume += RarArchive.unrar_MissingVolume;
            rarFileLocal.ExtractionProgress += RarArchive.unrar_ExtractionProgress;
            rarFileLocal.PasswordRequired += RarArchive.unrar_PasswordRequired;

            rarFileLocal.DestinationPath = this.TempExtractFolder;
            while (rarFileLocal.ReadHeader())
            {
                if (rarFileLocal.CurrentFile.FileName == fileName)
                {
                    rarFileLocal.Extract();
                    break;
                }
                else
                {
                    rarFileLocal.Skip();
                }
            }
            rarFileLocal.Close();
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
            DeleteAll(this.tempExtractFolder);
        }

        #endregion
    }

    class RarImage : ArchiveImage
    {
        RarArchive rarFile;
        string imgFileName;

        public RarImage(RarArchive rarFile, string imgFileName,int pageNo):base(null,pageNo)
        {
            this.rarFile = rarFile;
            this.imgFileName = imgFileName;
        }
        public override Stream ImageData
        {
            get
            {
                return this.rarFile[this.imgFileName];
            }
        }
    }
}
