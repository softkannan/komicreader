using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer
{
    public class EffectSettings : List<EffectSetting>
    {
        public Func<Task> EffectChanged { get; set; } = null;
    }

}