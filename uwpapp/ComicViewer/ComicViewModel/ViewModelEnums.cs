using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer.ComicViewModel
{
    public enum MousePageFlipType
    {
        Single,
        Double
    }

    public enum AutoRotationPreference
    {
        Off,
        On,
        PreferLandScape,
        PreferPortrait
    }

    public enum PageLinkType
    {
        Page,
        Bookmark
    }

    [Flags]
    public enum ImageEffect
    {
        None,
        Grey,
        Invert,
        Contrast,
        Flip,
        Brightness,
        Convolute
    }

    public enum ImageKernel
    {
        Blur,
        EdgeDetect,
        Emboss,
        Gradient,
        Sharpen
    }

    public enum PageResizeMode
    {
        System,
        Uniform,
        UniformToFill,
        Fill
    }

    /// <summary>
    /// Item Sizing type
    /// </summary>
    public enum ZoomType
    {
        Custom,
        FitWidth,
        Fit,
        FreeForm
    }

    /// <summary>
    /// Layout Mode
    /// </summary>
    public enum PanelMode
    {
        SinglePage,
        ContniousPage,
        DoublePage
    }

    public enum RotatePage
    {
        RotateNormal,
        Rotate90,
        Rotate180,
        Rotate270
    }
}