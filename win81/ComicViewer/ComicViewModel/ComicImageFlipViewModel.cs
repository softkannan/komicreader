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
    public class ComicImageFlipViewModel : IList, INotifyCollectionChanged
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

        public ComicImageViewModelList Source
        {
            get { return m_images; }
        }

        public ComicImageFlipViewModel(ComicImageViewModelList images)
        {
            this.m_images = images;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int IndexOf(object value)
        {
            return m_images.IndexOf(value as ComicImageViewModel);
            //throw new NotImplementedException();
        }

        public int Count
        {
            get { return m_images.Count; }
        }

        public object this[int index]
        {
            get
            {

                var retVal = m_images[index];

                if (!retVal.IsImagePopulated)
                {
                    BuildImageAsync(index, retVal as ComicImageViewModel);
                }

                return retVal;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        async void BuildImageAsync(int index, ComicImageViewModel image)
        {
            try
            {
                await image.BuildImageAsync();

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
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
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
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
