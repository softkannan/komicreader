using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace ComicViewer
{
    public class PageLink
    {
        public string DisplayName { get; set; }
        public PageLinkType Type { get; set; }
        public ComicImage Page { get; set; }
        public Brush BackColor { get; set; }
    }
}
