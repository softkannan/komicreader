using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ComicViewer
{
    /// <summary>
    /// View model used by scroll viewer or similar types of view
    /// </summary>
    public class ComicImageListViewModel : IList, INotifyCollectionChanged
    {
        //public async Task UpdateAttributes()
        //{
        //    for(int index=0;index < m_images.Count;index++)
        //    {
        //        if (m_images[index].IsImagePopulated)
        //        {
        //            m_images[index].UpdateImageAttribute();

        //            if (CollectionChanged != null)
        //            {
        //                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, m_images[index], null, index));
        //            }
        //        }
        //    }
        //}

        private ComicImageViewModelList m_images;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Data context proprty for scroll type viewer
        /// </summary>
        public ComicImageViewModelList Source
        {
            get { return m_images; }
        }
        public ComicImageListViewModel(ComicImageViewModelList images)
        {
            this.m_images = images;
        }
        /// <summary>
        /// check given value is present in the image list
        /// </summary>
        /// <param name="value">image</param>
        /// <returns>returns the found position</returns>
        public int IndexOf(object value)
        {
            return m_images.IndexOf(value as ComicImageViewModel);
            //throw new NotImplementedException();
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

                var retVal = m_images[index];

                if (!retVal.Image.IsImagePopulated)
                {
                    GetImageAtAsync(index, retVal as ComicImageViewModel);
                }

                return retVal;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Resolves image data for given page number
        /// </summary>
        /// <param name="index">page number</param>
        /// <param name="image">image object that needs to be resolved</param>
        private async void GetImageAtAsync(int index, ComicImageViewModel image)
        {
            try
            {
                await image.GetImageAsync();

                if (CollectionChanged != null)
                {
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, image, image, index));
                }
            }
            catch (Exception ex)
            {
                ShowError("Corrupt Comic File", ex);
            }
        }

        private async void ShowError(string title, Exception ex)
        {
            MessageDialog message = new MessageDialog(string.Format("{0} error : {1}", title, ex.Message), "Error");
            await message.ShowAsync();
        }


        #region ILists's Don't Care Methods

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotSupportedException(); }
        }

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
        #endregion
    }
}
