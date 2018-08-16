using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ComicViewer.ComicViewModel
{
    /// <summary>
    /// View model used by scroll viewer or similar types of view
    /// </summary>
    public class ComicImageIncrementalSource : ObservableCollection<ComicImageViewModel>,
    ISupportIncrementalLoading,
    INotifyPropertyChanged,
    IDisposable
    {
        private ComicImageViewModelList m_images;
        bool _busy = false;

        public ComicImageIncrementalSource(ComicImageViewModelList images)
        {
            this.m_images = images;
        }

        private int currentIndex = 0;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }

            _busy = true;

            CoreDispatcher coreDispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(async () =>
            {
                List<ComicImageViewModel> tempList = new List<ComicImageViewModel>();
                uint retCount = count;
                long lastIndex =  currentIndex + count;
                if(lastIndex > m_images.Count)
                {
                    retCount = (uint)(m_images.Count - currentIndex);
                    lastIndex = m_images.Count;
                    hasMoreItems = false;
                }
                while(currentIndex < lastIndex)
                {
                    tempList.Add(m_images[currentIndex]);
                    m_images[currentIndex].TriggerImageLoad();
                    currentIndex++;
                }

                await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var item in tempList)
                    {
                        this.Add(item);
                    }
                });

                return new LoadMoreItemsResult() { Count = count };

            }).AsAsyncOperation<LoadMoreItemsResult>();
        }

        private bool hasMoreItems = true;

        public bool HasMoreItems
        {
            get
            {
                return hasMoreItems;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ComicImageDynamicViewModel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}