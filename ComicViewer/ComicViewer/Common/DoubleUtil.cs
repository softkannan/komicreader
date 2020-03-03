using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace ComicViewer
{
    public static class DoubleUtil
    {
        // Fields
        internal const double DBL_EPSILON = 2.2204460492503131E-16;
        internal const float FLT_MIN = 1.175494E-38f;

        // Methods
        public static bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
            {
                return true;
            }
            double num = ((Math.Abs(value1) + Math.Abs(value2)) + 10.0) * 2.2204460492503131E-16;
            double num2 = value1 - value2;
            return ((-num < num2) && (num > num2));
        }

        public static bool AreClose(Point point1, Point point2)
        {
            return (AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y));
        }

        public static bool AreClose(Rect rect1, Rect rect2)
        {
            if (rect1.IsEmpty)
            {
                return rect2.IsEmpty;
            }
            return (((!rect2.IsEmpty && AreClose(rect1.X, rect2.X)) && (AreClose(rect1.Y, rect2.Y) && AreClose(rect1.Height, rect2.Height))) && AreClose(rect1.Width, rect2.Width));
        }

        public static bool AreClose(Size size1, Size size2)
        {
            return (AreClose(size1.Width, size2.Width) && AreClose(size1.Height, size2.Height));
        }

        public static bool AreClose(Vector vector1, Vector vector2)
        {
            return (AreClose(vector1.X, vector2.X) && AreClose(vector1.Y, vector2.Y));
        }

        public static int DoubleToInt(double val)
        {
            if (0.0 >= val)
            {
                return (int)(val - 0.5);
            }
            return (int)(val + 0.5);
        }

        public static bool GreaterThan(double value1, double value2)
        {
            return ((value1 > value2) && !AreClose(value1, value2));
        }

        public static bool GreaterThanOrClose(double value1, double value2)
        {
            if (value1 <= value2)
            {
                return AreClose(value1, value2);
            }
            return true;
        }

        public static bool IsBetweenZeroAndOne(double val)
        {
            return (GreaterThanOrClose(val, 0.0) && LessThanOrClose(val, 1.0));
        }

        public static bool IsNaN(double value)
        {
            NanUnion union = new NanUnion();
            union.DoubleValue = value;
            ulong num = union.UintValue & 18442240474082181120L;
            ulong num2 = union.UintValue & ((ulong)0xfffffffffffffL);
            if ((num != 0x7ff0000000000000L) && (num != 18442240474082181120L))
            {
                return false;
            }
            return (num2 != 0L);
        }

        public static bool IsOne(double value)
        {
            return (Math.Abs((double)(value - 1.0)) < 2.2204460492503131E-15);
        }

        public static bool IsZero(double value)
        {
            return (Math.Abs(value) < 2.2204460492503131E-15);
        }

        public static bool LessThan(double value1, double value2)
        {
            return ((value1 < value2) && !AreClose(value1, value2));
        }

        public static bool LessThanOrClose(double value1, double value2)
        {
            if (value1 >= value2)
            {
                return AreClose(value1, value2);
            }
            return true;
        }

        public static bool RectHasNaN(Rect r)
        {
            if ((!IsNaN(r.X) && !IsNaN(r.Y)) && (!IsNaN(r.Height) && !IsNaN(r.Width)))
            {
                return false;
            }
            return true;
        }
        public static double ValidateInputOffset(double offset, string parameterName)
        {
            if (IsNaN(offset))
            {
                throw new ArgumentOutOfRangeException(parameterName + "ScrollViewer_CannotBeNaN");
            }
            return Math.Max(0.0, offset);
        }

        public static Size MeasureArrangeHelper(Size inputSize, Size contentSize,Stretch stretch,StretchDirection direction)
        {
            Size size2 = ComputeScaleFactor(inputSize, contentSize, stretch, direction);
            return new Size(contentSize.Width * size2.Width, contentSize.Height * size2.Height);
        }

        internal static Size ComputeScaleFactor(Size availableSize, Size contentSize, Stretch stretch, StretchDirection stretchDirection)
        {
            double width = 1.0;
            double height = 1.0;
            bool flag = !Double.IsPositiveInfinity(availableSize.Width);
            bool flag2 = !Double.IsPositiveInfinity(availableSize.Height);
            if ((((stretch != Stretch.Uniform) && (stretch != Stretch.UniformToFill)) && (stretch != Stretch.Fill)) || (!flag && !flag2))
            {
                return new Size(width, height);
            }
            width = DoubleUtil.IsZero(contentSize.Width) ? 0.0 : (availableSize.Width / contentSize.Width);
            height = DoubleUtil.IsZero(contentSize.Height) ? 0.0 : (availableSize.Height / contentSize.Height);
            if (!flag)
            {
                width = height;
            }
            else if (!flag2)
            {
                height = width;
            }
            else
            {
                switch (stretch)
                {
                    case Stretch.Fill:
                        break;

                    case Stretch.Uniform:
                        {
                            double num3 = (width < height) ? width : height;
                            width = height = num3;
                            break;
                        }
                    case Stretch.UniformToFill:
                        {
                            double num4 = (width > height) ? width : height;
                            width = height = num4;
                            break;
                        }
                }
            }
            switch (stretchDirection)
            {
                case StretchDirection.UpOnly:
                    if (width < 1.0)
                    {
                        width = 1.0;
                    }
                    if (height < 1.0)
                    {
                        height = 1.0;
                    }
                    break;

                case StretchDirection.DownOnly:
                    if (width > 1.0)
                    {
                        width = 1.0;
                    }
                    if (height > 1.0)
                    {
                        height = 1.0;
                    }
                    break;

                case StretchDirection.Both:
                    break;
            }
            return new Size(width, height);
        }

        // Nested Types
        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion
        {
            // Fields
            [FieldOffset(0)]
            internal double DoubleValue;
            [FieldOffset(0)]
            internal ulong UintValue;
        }
    }
}
