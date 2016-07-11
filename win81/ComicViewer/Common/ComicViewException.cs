using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer
{
    public class ComicViewException : Exception
    {
        public ComicViewException()
        {
        }

        public ComicViewException(string message)
        : base(message)
        {
        }

        public ComicViewException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}