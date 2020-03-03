using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Data;
using System.Windows.Controls;

namespace ComicViewer
{
    public class ZoomTextRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult retVal;
            string txtValue = value as string;
            if (ZoomToSizingType.GetItemSizeType(txtValue) == ItemSizingType.Custom)
            {
                double tempVal;
                if (Double.TryParse(txtValue,out tempVal))
                {
                    retVal = new ValidationResult(true, null);
                }
                else
                {
                    retVal = new ValidationResult(false, "Please Enter Numbers");
                }
            }
            else
            {
                retVal = new ValidationResult(true, null);
            }
            return retVal;
        }
    }
    public class IndexToPageNo : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value + 1;
        }

        #endregion
    }
    public class RotatePageToBool : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "Normal":
                    return (RotatePage)value == RotatePage.RotateNormal;
                case "90":
                    return (RotatePage)value == RotatePage.Rotate90;
                case "180":
                    return (RotatePage)value == RotatePage.Rotate180;
                case "270":
                    return (RotatePage)value == RotatePage.Rotate90CounterClock;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class RotatePageToTransForm : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((RotatePage)value)
            {
                case RotatePage.Rotate90:
                    {
                        TransformGroup myTransformGroup = new TransformGroup();
                        myTransformGroup.Children.Add(new RotateTransform(90));
                        return myTransformGroup;
                    }
                case RotatePage.Rotate180:
                    {
                        TransformGroup myTransformGroup = new TransformGroup();
                        myTransformGroup.Children.Add(new RotateTransform(180));
                        return myTransformGroup;
                    }
                case RotatePage.Rotate90CounterClock:
                    {
                        TransformGroup myTransformGroup = new TransformGroup();
                        myTransformGroup.Children.Add(new RotateTransform(270));
                        return myTransformGroup;
                    }
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class PanelModeToBool : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "Single":
                    return (PanelMode)value == PanelMode.SinglePage;
                case "Continuous":
                    return (PanelMode)value ==  PanelMode.ContniousPage;
                case "Double":
                    return (PanelMode)value == PanelMode.DoublePage;
                case "DoubleContinuous":
                    return (PanelMode)value == PanelMode.DoubleContniousPage;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class ImageEffectToBool : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "None":
                    return (ImageEffect)value == ImageEffect.None;
                case "Grey":
                    return (ImageEffect)value == ImageEffect.Grey;
                case "BW":
                    return (ImageEffect)value == ImageEffect.BW;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class ZoomToSizingType : IValueConverter
    {
        static Dictionary<string, ItemSizingType> txtToSizing = new Dictionary<string, ItemSizingType>();
        static Dictionary<ItemSizingType, string> sizingToText = new Dictionary<ItemSizingType, string>();
        static ZoomToSizingType()
        {
            txtToSizing.Add("Actul Size",ItemSizingType.Original);
            txtToSizing.Add("Fit Page", ItemSizingType.Fit);
            txtToSizing.Add("Fit Width", ItemSizingType.FitWidth);
            txtToSizing.Add("8.33%", ItemSizingType.Z8DOT33);
            txtToSizing.Add("12.5%", ItemSizingType.Z12DOT5);
            txtToSizing.Add("25%", ItemSizingType.Z25);
            txtToSizing.Add("33.33%", ItemSizingType.Z33DOT33);
            txtToSizing.Add("50%", ItemSizingType.Z50);
            txtToSizing.Add("66.67%", ItemSizingType.Z66DOT67);
            txtToSizing.Add("75%", ItemSizingType.Z75);
            txtToSizing.Add("100%", ItemSizingType.Z100);
            txtToSizing.Add("125%", ItemSizingType.Z125);
            txtToSizing.Add("150%", ItemSizingType.Z150);
            txtToSizing.Add("200%", ItemSizingType.Z200);
            txtToSizing.Add("300%", ItemSizingType.Z300);
            txtToSizing.Add("400%", ItemSizingType.Z400);
            txtToSizing.Add("600%", ItemSizingType.Z600);
            txtToSizing.Add("800%", ItemSizingType.Z800);
            txtToSizing.Add("1200%", ItemSizingType.Z1200);
            txtToSizing.Add("1600%", ItemSizingType.Z1600);
            txtToSizing.Add("3200%", ItemSizingType.Z3200);
            txtToSizing.Add("6400%", ItemSizingType.Z6400);

            

            foreach (KeyValuePair<string, ItemSizingType> item in txtToSizing)
            {
                sizingToText.Add(item.Value, item.Key);
            }
                   
        }
        public static ItemSizingType GetItemSizeType(string text)
        {
            ItemSizingType retVal;
            if (!txtToSizing.TryGetValue(text, out retVal))
            {
                retVal = ItemSizingType.Custom;
            }
            return retVal;
        }
        public static string GetText(ItemSizingType type)
        {
            string retVal;
            if (!sizingToText.TryGetValue(type, out retVal))
            {
                retVal = "Fit Width";
            }
            return retVal;
        }
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int retVal = (int)GetItemSizeType(value as string);
            if(retVal > 2)
            {
                retVal = -1;
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return GetText((ItemSizingType)value);
        }

        #endregion
    }
    public class ZoomTextToBool : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "Fit":
                    return (string)value == "Fit Page";
                case "Original":
                    return (string)value == "Actul Size";
                case "FitWidth":
                    return (string)value == "Fit Width";
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class ArchiveImageToThumbnailImage : IValueConverter
    {
        const int ThumbNailWidth = 100;
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Stream tempImg = value as Stream;
            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.DecodePixelWidth = ThumbNailWidth;
            bit.CacheOption = BitmapCacheOption.OnLoad;
            bit.StreamSource = tempImg;
            bit.EndInit();
            return bit;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ArchiveImageToPageNo : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int tempImg = (int)value;
            return tempImg.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class ArchiveImageToImage : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Stream data = value as Stream;
            BitmapImage tempImg = new BitmapImage();
            tempImg.BeginInit();
            tempImg.CacheOption = BitmapCacheOption.OnLoad;
            tempImg.StreamSource = data;
            tempImg.EndInit();
            return tempImg;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class PanelModeToInt : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int tempValue = (int)value;
            return tempValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PanelMode tempValue = (PanelMode)value;
            return tempValue;
        }

        #endregion
    }

    public class SizingTypeToInt : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int tempValue = (int)value;
            return tempValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ItemSizingType tempValue = (ItemSizingType)value;
            return tempValue;
        }

        #endregion
    }

    public class ImageEffectTypeToInt : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int tempValue = (int)value;
            return tempValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageEffect tempValue = (ImageEffect)value;
            return tempValue;
        }

        #endregion
    }
    static class ImagefileUtility
    {
           

        public static BitmapImage GetImageThumbNails(Stream fileData)
        {
            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.DecodePixelWidth = 100;
            bit.CacheOption = BitmapCacheOption.OnLoad;
            bit.StreamSource = fileData;
            bit.EndInit();
            return bit;
        }
        public static BitmapImage GetImageSource(Stream fileData)
        {
            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.CacheOption = BitmapCacheOption.OnLoad;
            bit.StreamSource = fileData;
            bit.EndInit();
            return bit;
        }
        public static ImageSource GetImageSource1(Stream fileData, string extn)
        {

            if (extn == ".jpeg" || extn == ".jpg" || extn == ".jpe" || extn == ".jfif")
            {
                BitmapDecoder jpegImg = JpegBitmapDecoder.Create(fileData, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);
                BitmapImage bit = new BitmapImage();
                bit.BeginInit();
                bit.DecodePixelWidth=50;
                bit.CacheOption = BitmapCacheOption.OnLoad;
                bit.StreamSource = fileData;
                bit.EndInit();
                
                return jpegImg.Frames[0];
            }
            else if (extn == ".png")
            {
                BitmapDecoder pngImg = PngBitmapDecoder.Create(fileData, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);

                return pngImg.Frames[0];
            }
            else if (extn == ".tif" || extn == ".tiff")
            {
                BitmapDecoder tifImg = TiffBitmapDecoder.Create(fileData, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);

                return tifImg.Frames[0];
            }
            else if (extn == ".gif")
            {
                BitmapDecoder gifImg = GifBitmapDecoder.Create(fileData, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);

                return gifImg.Frames[0];
            }
            else if (extn == ".bmp" || extn == ".dib")
            {
                BitmapDecoder bmpImg = BitmapDecoder.Create(fileData, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);

                return bmpImg.Frames[0];
            }
            throw new NotSupportedException("File Type is NotSupported");
        }
        public static bool IsImageFile(string extn)
        {
            switch (extn)
            {
                case ".jpeg":
                case ".jpg":
                case ".jpe":
                case ".jfif":
                case ".png":
                case ".tif":
                case ".tiff":
                case ".gif":
                case ".bmp":
                case ".dib":
                    return true;
                default:
                    return false;
            }
        }
    }
}
