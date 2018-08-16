using ComicViewer.ComicViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ComicViewer
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GoToPage : Page
    {
        public GoToPage()
        {
            this.InitializeComponent();
        }

        public Action<uint> GotoPage { get; set; } = null;

        private uint _currentPage = 1;
        private ComicImageViewModelList _bookPages;
        private List<uint> _bookmarks;
        private List<PageLinkViewModel> _pageListSource;
        private bool _canHandleSelection = false;

        private void UpdatePages(ComicImageViewModelList argPages)
        {
            _canHandleSelection = false;
            try
            {
                if (this._bookPages != argPages)
                {
                    this._bookPages = argPages;

                    _pageListSource = new List<PageLinkViewModel>();

                    //_pageListSource.AddRange(from t in bookmarks from t1 in pages where t == t1.PageNo
                    //                         select new PageLinkViewModel() { Page = t1, DisplayName = string.Format("B{0}", t1.PageNo), Type = PageLinkType.Bookmark});

                    for (int index = 0; index < _bookPages.Count; index++)
                    {
                        _pageListSource.Add(new PageLinkViewModel() { Page = _bookPages[index], DisplayName = string.Format("P{0}", _bookPages[index].PageNo), Type = PageLinkType.Page });
                    }
                }

                gotoPanel.ItemsSource = _pageListSource;
            }
            finally
            {
                _canHandleSelection = true;
            }
        }

        public void SetPages(ComicImageViewModelList argPages, List<uint> argBookmarks, uint argCurrentPage)
        {
            this._bookmarks = argBookmarks;
            this._currentPage = argCurrentPage;
            UpdatePages(argPages);
        }

        private void bttn_Click(object sender, RoutedEventArgs e)
        {
            CloseFlyout(sender, e);

            Button tempBttn = sender as Button;

            if (tempBttn == null)
            {
                return;
            }

            PageLinkViewModel tempImage = tempBttn.Tag as PageLinkViewModel;

            if (tempImage == null)
            {
                return;
            }

            if (GotoPage != null)
            {
                GotoPage(tempImage.Page.PageNo);
            }
        }

        ///// <summary>
        ///// Populates the page with content passed during navigation.  Any saved state is also
        ///// provided when recreating a page from a prior session.
        ///// </summary>
        ///// <param name="navigationParameter">The parameter value passed to
        ///// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        ///// </param>
        ///// <param name="pageState">A dictionary of state preserved by this page during an earlier
        ///// session.  This will be null the first time a page is visited.</param>
        //protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        //{
        //}

        ///// <summary>
        ///// Preserves state associated with this page in case the application is suspended or the
        ///// page is discarded from the navigation cache.  Values must conform to the serialization
        ///// requirements of <see cref="SuspensionManager.SessionState"/>.
        ///// </summary>
        ///// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        //protected override void SaveState(Dictionary<String, Object> pageState)
        //{
        //}

        private void CloseFlyout(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Popup)
            {
                (this.Parent as Popup).IsOpen = false;
            }
        }

        private void bttnMarkBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (_bookmarks != null)
            {
                if (!_bookmarks.Contains(_currentPage))
                {
                    _bookmarks.Add(_currentPage);
                    //UpdatePages();
                }
            }
        }

        private void bttnDeleteAllBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (_bookmarks != null)
            {
                _bookmarks.Clear();
                //UpdatePages();
            }
        }

        private void gotoPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_canHandleSelection)
            {
                CloseFlyout(sender, e);

                PageLinkViewModel tempImage = gotoPanel.SelectedItem as PageLinkViewModel;

                if (tempImage == null)
                {
                    return;
                }

                if (GotoPage != null)
                {
                    GotoPage(tempImage.Page.PageNo);
                }
            }
        }

        private void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _canHandleSelection = false;
            try
            {
                var curPage = _pageListSource.FirstOrDefault((item) => item.Page.PageNo == _currentPage);
                if (curPage != null)
                {
                    gotoPanel.SelectedItem = curPage;
                    gotoPanel.ScrollIntoView(curPage);
                }
            }
            finally
            {
                _canHandleSelection = true;
            }
        }
    }
}