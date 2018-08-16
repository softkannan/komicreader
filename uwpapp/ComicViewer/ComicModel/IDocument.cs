using ComicViewer.ComicViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer.ComicModel
{
    public interface IDocument : IDisposable
    {
        uint TotalPages { get; }

        void WriteTo(ComicImageViewModelList listWriteTo);
    }
}
