using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer
{
    public class EffectSetting
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasValue { get; set; }
        public double Value { set; get; }
        public ImageEffect Type { get; set; }
    }
}
