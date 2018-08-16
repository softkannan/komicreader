using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ComicViewer.Control
{
    public class ContiniousView : ListView
    {
        private ScrollViewer InternalScrollViewer { get; set; }

        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
        {
            InternalScrollViewer = this.GetElement<ScrollViewer>();

            return base.MeasureOverride(availableSize);
        }

        protected override void OnKeyDown(Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (InternalScrollViewer != null)
            {
                if (e.Key == Windows.System.VirtualKey.Up)
                {
                    e.Handled = true;
                    this.InternalScrollViewer.ChangeView(null, this.InternalScrollViewer.VerticalOffset - 0.05, null);
                }
                else if (e.Key == Windows.System.VirtualKey.Down)
                {
                    e.Handled = true;

                    this.InternalScrollViewer.ChangeView(null, this.InternalScrollViewer.VerticalOffset + 0.05, null);
                }
            }
            base.OnKeyDown(e);
        }
    }
}