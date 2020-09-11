using ComicViewer.ComicViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer.ComicModel
{
    public class MuPDFFile : IDocument
    {
        public uint TotalPages => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void WriteTo(ComicImageViewModelList listWriteTo)
        {
            throw new NotImplementedException();
        }
    }
}
