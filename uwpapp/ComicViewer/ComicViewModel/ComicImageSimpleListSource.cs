using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Data;

namespace ComicViewer.ComicViewModel
{
    /// <summary>
    /// View model used by scroll viewer or similar types of view
    /// </summary>
    public class ComicImageSimpleListSource : INotifyCollectionChanged, System.Collections.IList
    {
        private ComicImageViewModelList m_images;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ComicImageSimpleListSource(ComicImageViewModelList images)
        {
            this.m_images = images;
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Contains(object value)
        {
            return IndexOf(value) != -1;
        }

        /// <summary>
        /// check given value is present in the image list
        /// </summary>
        /// <param name="value">image</param>
        /// <returns>returns the found position</returns>
        public int IndexOf(object value)
        {
            return m_images.IndexOf(value as ComicImageViewModel); 
        }

        /// <summary>
        /// Returns total number of images available in the comic book
        /// </summary>
        public int Count
        {
            get { return m_images.Count; }
        }

        /// <summary>
        /// This method will be called by the any scroll viewer type views
        /// </summary>
        /// <param name="index">page index</param>
        /// <returns>returns the image data</returns>
        public object this[int index]
        {
            get
            {
                //this method is always called in side thread / task by control
                var image = m_images[index];
                image.TriggerImageLoad();
                ComicInfo.Inst.NotifyPageChange();
                return image;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private void ImageDecoded(ItemChangedEventArgs args)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            if (!dispatcher.HasThreadAccess)
            {
                //UpdateUICurrentPage();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                         () =>
                         {
                                 // update your UI here
                                 CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.OldItem, args.NewItem, args.ItemIndex));

                         });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.OldItem, args.NewItem, args.ItemIndex));
            }
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            Debug.WriteLine("RangesChanged FirstIndex {0} LastIndex {1} Length {2}", visibleRange.FirstIndex , visibleRange.LastIndex, visibleRange.Length);
        }

        private void PrefetchImage(int index)
        {
            if (index > 0 && index < m_images.Count)
            {
                m_images[index].TriggerImageLoad();
            }
        }

        private async void ShowError(string title, Exception ex)
        {
            MessageDialog message = new MessageDialog(string.Format("{0} error : {1}", title, ex.Message), "Error");
            await message.ShowAsync();
        }

        public void Dispose()
        {
        }

        #region ILists's Don't Care Methods
        
        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { throw new NotSupportedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void SelectRange(ItemIndexRange itemIndexRange)
        {
            Debug.WriteLine("RangesChanged FirstIndex {0} LastIndex {1} Length {2}", itemIndexRange.FirstIndex, itemIndexRange.LastIndex, itemIndexRange.Length);
        }

        public void DeselectRange(ItemIndexRange itemIndexRange)
        {
            Debug.WriteLine("RangesChanged FirstIndex {0} LastIndex {1} Length {2}", itemIndexRange.FirstIndex, itemIndexRange.LastIndex, itemIndexRange.Length);
        }

        public bool IsSelected(int index)
        {
            Debug.WriteLine("IsSelected Index {0}", index);
            return false;
        }

        public IReadOnlyList<ItemIndexRange> GetSelectedRanges()
        {
            return null;
        }
        #endregion ILists's Don't Care Methods
    }
}