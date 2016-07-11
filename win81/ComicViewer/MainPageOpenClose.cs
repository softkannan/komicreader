using SharpCompress.Archive.Rar;
using SharpCompress.Archive.Zip;
using SharpCompress.Reader;
using SharpCompress.Reader.Rar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Archive.Tar;
using SharpCompress.Archive;

namespace ComicViewer
{
    public partial class MainPage
    {

        private async Task OpenComicAsync()
        {
            CloseComic();

            var filePicker = new FileOpenPicker();
            filePicker.SettingsIdentifier = "KomicReader";

            filePicker.FileTypeFilter.Add(".cbr");
            filePicker.FileTypeFilter.Add(".cbz");
            filePicker.FileTypeFilter.Add(".rar");
            filePicker.FileTypeFilter.Add(".zip");
            filePicker.FileTypeFilter.Add(".7z");
            filePicker.FileTypeFilter.Add(".cb7");
            filePicker.FileTypeFilter.Add(".tar");
            filePicker.FileTypeFilter.Add(".cbt");

            StorageFile comicFile = await filePicker.PickSingleFileAsync();
              
            await OpenStorageFileAsync(comicFile);

            await ShowPage();
        }

        private async Task OpenComicAsync(IStorageItem arg)
        {
            CloseComic();

            StorageFile comicFile = (StorageFile)arg;

            await OpenStorageFileAsync(comicFile);

            await ShowPage();
        }

        public async Task OpenStorageFileAsync(StorageFile comicFile)
        {
            if (comicFile != null)
            {
                var comicStream = await comicFile.OpenAsync(FileAccessMode.Read);

                CloseComic();

                try
                {

                    comicFileReader = ArchiveFactory.Open(comicStream.AsStreamForRead());

                    //switch (comicFile.FileType.ToLower())
                    //{
                    //    case ".zip":
                    //    case ".cbz":
                    //        comicFileReader = ZipArchive.Open(comicStream.AsStreamForRead());
                    //        break;
                    //    case ".rar":
                    //    case ".cbr":
                    //        comicFileReader = RarArchive.Open(comicStream.AsStreamForRead());
                    //        break;
                    //    case ".cb7":
                    //    case ".7z":
                    //        comicFileReader = SevenZipArchive.Open(comicStream.AsStreamForRead());
                    //        break;
                    //    case ".tar":
                    //    case ".cbt":
                    //         comicFileReader = TarArchive.Open(comicStream.AsStreamForRead());
                    //        break;
                    //}
                }
                catch (Exception)
                {
                    comicFileReader = null;
                    throw;
                }

                if (comicFileReader != null)
                {
                    RestoreSettings(comicFile.Name);

                    var files = (from t in comicFileReader.Entries where t.IsImageFile() == true orderby t.FilePath select t).ToList();
                    Pages = new ComicImageViewModelList();
                    int tempPage = 1;
                    foreach (var item in files)
                    {
                        Pages.Add(new ComicImageViewModel(item, tempPage));
                        tempPage++;
                    }
                    LastPage = tempPage - 1;
                    ZoomFactor = 1;
                                       
                    if (Pages.Count < 1)
                    {
                        throw new InvalidDataException("Unable find any comic strips");
                    }

                    //if (PanelMode == ComicViewer.PanelMode.ContniousPage)
                    //{
                    //    PanelMode = ComicViewer.PanelMode.SinglePage;

                    //    Zoom = ZoomType.FitWidth;
                    //}

                    //comicContiniousScroll.DataContext = null;
                    //comicContiniousScroll.ItemsSource = Pages;
                    //PanelMode = ComicViewer.PanelMode.ContniousPage;
                }
                else
                {
                    throw new InvalidOperationException("unable to open the comic file");
                }
            }
        }

        private void CloseComic()
        {
            continuousView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bookView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (comicFileReader != null)
            {
                UpdateCurrentPage();
                
                Pages.Exec((item) => item.Next = null);

                ReleaseImage();
                CurrentSource = null;
                Pages = null;
                LastPage = 0;
                CurrentPage = 0;
                currentSetting = null;
                comicFileReader.Dispose();
                comicFileReader = null;
            }
        }
    }
}
