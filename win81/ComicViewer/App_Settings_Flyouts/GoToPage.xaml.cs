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

        public Func<int, Task> GotoPage { get; set; } = null;

        private int currentPage = 1;
        private List<ComicImageViewModel> pages;
        private List<int> bookmarks;

        private void UpdatePages()
        {
            List<PageLinkViewModel> tempList = new List<PageLinkViewModel>();

            var tempBrush = new SolidColorBrush(Colors.Chocolate);

            tempList.AddRange(from t in bookmarks from t1 in pages where t == t1.PageNo select new PageLinkViewModel() { Page = t1, DisplayName = string.Format("B{0}", t1.PageNo), Type = PageLinkType.Bookmark, BackColor = tempBrush });

            var query = (from t in pages select new PageLinkViewModel() { Page = t, DisplayName = string.Format("P{0}", t.PageNo), Type = PageLinkType.Page, BackColor = gotoGrid.Background }).ToList();

            tempList.AddRange(query);

            var curPage = query.FirstOrDefault((item) => item.Page.PageNo == currentPage);

            if (curPage != null)
            {
                curPage.BackColor = new SolidColorBrush(Colors.Blue);
            }

            //txtCurrentPageNo.Text = currentPage.ToString();

            //var lastPage = pages.Max((item) => item.PageNo);

            //txtTotalPageNo.Text = lastPage.ToString();

            gotoPanel.ItemsSource = tempList;
        }

        public void SetPages(List<ComicImageViewModel> argPages, List<int> argBookmarks, int argCurrentPage)
        {
            this.pages = argPages;
            this.bookmarks = argBookmarks;
            this.currentPage = argCurrentPage;

            UpdatePages();
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
                (this.Parent as Popup).IsOpen = false;
        }

        private void bttnMarkBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (bookmarks != null)
            {
                if (!bookmarks.Contains(currentPage))
                {
                    bookmarks.Add(currentPage);
                    UpdatePages();
                }
            }
        }

        private void bttnDeleteAllBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (bookmarks != null)
            {
                bookmarks.Clear();
                UpdatePages();
            }
        }
    }
}