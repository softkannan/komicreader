using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ComicViewer
{
    public static class CBCursors
    {
        public static Cursor HandNormal { get; private set; }
        public static Cursor HandPressed { get; private set; }

        static CBCursors()
        {
            HandNormal = new Cursor(System.Reflection.Assembly.GetAssembly(typeof(CBCursors)).GetManifestResourceStream("ComicViewer.Cursors.HandNormal.cur"));
            HandPressed = new Cursor(System.Reflection.Assembly.GetAssembly(typeof(CBCursors)).GetManifestResourceStream("ComicViewer.Cursors.HandPressed.cur"));
        }
    }
}
