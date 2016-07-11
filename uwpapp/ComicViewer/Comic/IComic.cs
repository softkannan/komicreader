using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ComicViewer
{
    public interface IComic
    {
        ComicImages Pages { get; }
        Size PageSize { get; }
        void Close();
    }
}
