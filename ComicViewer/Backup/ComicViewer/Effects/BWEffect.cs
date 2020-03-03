using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;

namespace ComicViewer.Effects
{
    public class BWEffect : ShaderCustomEffect
    {
        private static PixelShader _pixelShader = new PixelShader() { UriSource = new Uri(@"pack://application:,,,/ComicViewer;component/Effects/GrayscaleEffect.ps") };

        public BWEffect()
        {
            PixelShader = _pixelShader;
        }

    }
}
