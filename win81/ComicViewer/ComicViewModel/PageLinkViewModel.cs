using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace ComicViewer
{
    public class PageLinkViewModel
    {
        public string DisplayName { get; set; }
        public PageLinkType Type { get; set; }
        public ComicImageViewModel Page { get; set; }
        public Brush BackColor { get; set; }
    }
}