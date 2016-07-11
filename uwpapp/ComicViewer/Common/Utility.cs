using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicViewer
{

    public static class ComicViewerUtility
    {
        public static T GetElement<T>(this UIElement uiElement) where T: class
        {
            T retVal = null;

            int totalChildern = VisualTreeHelper.GetChildrenCount(uiElement);

            for (int index = 0; index < totalChildern; index++)
            {
                var tempObj = VisualTreeHelper.GetChild(uiElement, index) as T;

                if (tempObj != null)
                {
                    retVal = tempObj;
                    break;
                }
            }

            return retVal;
        }

        public static float ParseText(this string textVal, float defaultVal)
        {
            float tempVal;

            if (float.TryParse(textVal, out tempVal))
            {
                return tempVal;
            }
            return defaultVal;
        }

        public static int ParseText(this string textVal, int defaultVal)
        {
            int tempVal;

            if (int.TryParse(textVal, out tempVal))
            {
                return tempVal;
            }
            return defaultVal;
        }

        public static bool ValidateMinMax(this TextBox txtUnder, double min, double max)
        {
            double tempVal;

            if (double.TryParse(txtUnder.Text, out tempVal))
            {
                if (tempVal < max && tempVal > min)
                {
                    txtUnder.Background = new SolidColorBrush(Colors.White);
                    return true;
                }
            }

            txtUnder.Background = new SolidColorBrush(Colors.Red);
            return false;
        }

        public static bool ValidateMinMax(this TextBox txtUnder, int min, int max)
        {
            int tempVal;

            if (int.TryParse(txtUnder.Text, out tempVal))
            {
                if (tempVal < max && tempVal > min)
                {
                    txtUnder.Background = new SolidColorBrush(Colors.White);
                    return true;
                }
            }

            txtUnder.Background = new SolidColorBrush(Colors.Red);
            return false;
        }

        public static void Exec<TSource>(this IEnumerable<TSource> source, Action<TSource, int> predicate)
        {
            int count = 0;
            foreach (TSource item in source)
            {
                predicate(item, count);

                count++;
            }
        }

        public static void Exec<TSource>(this IEnumerable<TSource> source, Action<TSource> predicate)
        {
            foreach (TSource item in source)
            {
                predicate(item);
            }
        }

        public static List<int> ToIntList(this string input)
        {
            var retVal = new List<int>();

            if (!string.IsNullOrWhiteSpace(input))
            {
                string[] tempTokens = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (tempTokens != null && tempTokens.Length > 0)
                {
                    foreach (var item in tempTokens)
                    {
                        retVal.Add(int.Parse(item));
                    }
                }
            }

            return retVal;
        }

        public static string ToCSString(this List<int> input)
        {
            var retVal = new StringBuilder();

            if (input != null && input.Count > 0)
            {
                foreach (var item in input)
                {
                    retVal.Append(item.ToString());
                    retVal.Append(',');
                }
            }

            return retVal.ToString().TrimEnd(',');
        }

        public static bool IsImageFile(this IArchiveEntry entry)
        {
            switch (Path.GetExtension(entry.FilePath).ToLower())
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
