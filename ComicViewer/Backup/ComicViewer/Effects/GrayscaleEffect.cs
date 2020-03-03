using System;
using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;

namespace ComicViewer.Effects
{
    public class GrayscaleEffect : ShaderCustomEffect
    {
        private static PixelShader _pixelShader = new PixelShader() { UriSource = new Uri(@"pack://application:,,,/ComicViewer;component/Effects/GrayscaleEffect.ps") };

        public GrayscaleEffect()
        {
            PixelShader = _pixelShader;
        }
    }
}
