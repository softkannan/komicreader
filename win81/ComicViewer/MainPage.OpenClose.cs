using ComicViewer.ComicModel;
using SharpCompress.Archive;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ComicViewer
{
    public partial class MainPage
    {
        public async Task OpenStorageFileAsync(StorageFile comicFile)
        {
            if (comicFile != null)
            {
                var comicStream = await comicFile.OpenAsync(FileAccessMode.Read);

                CloseComic();

                try
                {
                    comicFileReader = ArchiveFactory.Open(comicStream.AsStreamForRead());
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
                        Pages.Add(new ComicImageViewModel(new ComicImage(item), tempPage));
                        tempPage++;
                    }
                    LastPage = tempPage - 1;
                    ZoomFactor = 1;

                    if (Pages.Count < 1)
                    {
                        throw new InvalidDataException("Unable find any comic strips");
                    }
                }
                else
                {
                    throw new InvalidOperationException("unable to open the comic file");
                }
            }
        }

        private async void bttnOpen_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            while (true)
            {
                Exception tempEx = null;

                try
                {
                    Busy();

                    await OpenComicAsync();
                }
                catch (Exception ex)
                {
                    tempEx = ex;
                }
                finally
                {
                    NotBusy();
                }

                if (tempEx != null)
                {
                    await ShowErrorAsync("File Open", tempEx);
                }
                else
                {
                    break;
                }
            }
        }

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