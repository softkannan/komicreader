using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrendanGrant.Helpers.FileAssociation;

namespace ComicViewer
{
    static class FileTypeRegister
    {
        public static void RegisterFile(string appFolder)
        {
            {
                FileAssociationInfo fa = new FileAssociationInfo(".cbr");

                if (!fa.Exists)
                {
                    fa.Create("ComicBookRAR", PerceivedTypes.Compressed, "", null);
                }

                ProgramAssociationInfo prInfo = new ProgramAssociationInfo(fa.ProgID);

                if (!prInfo.Exists)
                {
                    prInfo.Create("Comic Book RAR", EditFlags.None, new ProgramVerb("open", appFolder + "\\ComicViewer.exe \"%1\""));
                    prInfo.DefaultIcon = new ProgramIcon(appFolder + "\\cbr.ico");
                }
            }
            {
                FileAssociationInfo fa = new FileAssociationInfo(".cbz");

                if (!fa.Exists)
                {
                    fa.Create("ComicBookZIP", PerceivedTypes.Compressed, "", null);
                }

                ProgramAssociationInfo prInfo = new ProgramAssociationInfo(fa.ProgID);

                if (!prInfo.Exists)
                {
                    prInfo.Create("Comic Book ZIP", EditFlags.None, new ProgramVerb("open", appFolder + "\\ComicViewer.exe \"%1\""));
                    prInfo.DefaultIcon = new ProgramIcon(appFolder + "\\cbz.ico");
                }
            }
        }
    }
}
