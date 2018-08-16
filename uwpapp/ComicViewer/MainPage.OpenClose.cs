using ComicViewer.ComicModel;
using ComicViewer.ComicViewModel;
using SharpCompress.Archives;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Pdf;
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

                CloseComic();

                var comicStream = await comicFile.OpenAsync(FileAccessMode.Read);

                try
                {
                    if (comicFile.FileType == ".pdf")
                    {
                        _documentReader = new PDFFile(await PdfDocument.LoadFromStreamAsync(comicStream));
                    }
                    else
                    {
                        _documentReader = new ComicFile(comicStream);
                    }
                }
                catch (Exception)
                {
                    _documentReader = null;
                    throw new InvalidOperationException("Unable to open archive file");
                }

                if (_documentReader != null)
                {
                    ComicInfo.Inst.CurrentPage = 0;
                    ComicInfo.Inst.RestoreSettings(comicFile.Name);
                    //create list of image files list model object
                    var listOfPagesInFile = new ComicImageViewModelList();
                    _documentReader.WriteTo(listOfPagesInFile);
                    if (listOfPagesInFile.Count < 1)
                    {
                        throw new InvalidDataException("Unable find any comic strips");
                    }
                    else
                    {
                        txtFilename.Text = Path.GetFileName(comicFile.Name);
                        ComicInfo.Inst.LastPage = _documentReader.TotalPages - 1;
                        ComicInfo.Inst.ZoomFactor = 1;

                        Pages = listOfPagesInFile;
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
            filePicker.FileTypeFilter.Add(".pdf");

            StorageFile comicFile = await filePicker.PickSingleFileAsync();

            await OpenStorageFileAsync(comicFile);

            ShowPage();
        }

        private async Task OpenComicAsync(IStorageItem arg)
        {
            CloseComic();

            StorageFile comicFile = (StorageFile)arg;

            if (comicFile != null)
            {
                await OpenStorageFileAsync(comicFile);

                ShowPage();
            }
        }

        private void CloseComic()
        {
            continuousView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bookView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            bookFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageFlipView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (_documentReader != null)
            {
                Pages.Exec((item) => item.UnsetImageData());

                ReleaseImage();
                CurrentSource = null;
                Pages = null;
                ComicInfo.Inst.LastPage = 0;
                ComicInfo.Inst.CurrentPage = 0;
                _documentReader.Dispose();
                _documentReader = null;
                //simple update UI
                InternalPropertyChanged("CurrentPage");
            }
        }
    }
}