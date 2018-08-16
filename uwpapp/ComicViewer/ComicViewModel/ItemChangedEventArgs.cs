using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer.ComicViewModel
{
    public class ItemChangedEventArgs
    {
        public ComicImageViewModel OldItem { get; set; }
        public ComicImageViewModel NewItem { get; set; }
        public int ItemIndex { get; set; }
    }
}
