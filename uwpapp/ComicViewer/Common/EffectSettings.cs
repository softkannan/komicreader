using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer.Common
{
    public class EffectSettings : List<EffectSetting>
    {
        public Action EffectChanged { get; set; } = null;
    }

}