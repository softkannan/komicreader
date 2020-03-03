using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace ComicViewer
{
    static class ArchiveUtility
    {
        static public IArchive OpenArchive(string fileInfo)
        {
            IArchive retVal = null;
            try
            {
                string tempExtn = Path.GetExtension(fileInfo).ToLower();

                if (tempExtn == ".cbz" || tempExtn == ".zip")
                {
                    retVal = new ZipArchive(fileInfo);
                }
                else if (tempExtn == ".cbr" || tempExtn == ".rar")
                {
                    retVal = new RarArchive(fileInfo);
                }
                else if (Directory.Exists(fileInfo))
                {
                    retVal = new FolderArchive(fileInfo);
                }
                else
                {
                    System.Windows.MessageBox.Show("File Not Supported");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            return retVal;
        }
    }
}
