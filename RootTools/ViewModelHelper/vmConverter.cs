using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using RootTools;
using MaterialDesignThemes.Wpf;
using System.Globalization;
using RootTools.Control.Ajin;

namespace ViewConverter
{

    public class ContentToMarginConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Thickness(0, 0, -((ContentPresenter)value).ActualHeight, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
    public class ContentIconToLogLevelConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PackIconKind result;
            string str = value.ToString();
            LogView.eLogType type = LogView.eLogType.ENG;
            if (Enum.TryParse(value.ToString(), out type))
            {
                switch (type)
                {
                    case LogView.eLogType.FNC:
                        result = MaterialDesignThemes.Wpf.PackIconKind.VectorPoint;
                        break;
                    case LogView.eLogType.XFR:
                        result = MaterialDesignThemes.Wpf.PackIconKind.Motorbike;
                        break;
                    case LogView.eLogType.PRC:
                        result = MaterialDesignThemes.Wpf.PackIconKind.Monitor;
                        break;
                    case LogView.eLogType.ALM:
                        result = MaterialDesignThemes.Wpf.PackIconKind.Error;
                        break;
                    case LogView.eLogType.CFG:
                        result = MaterialDesignThemes.Wpf.PackIconKind.Wrench;
                        break;
                    case LogView.eLogType.COMM:
                        result = MaterialDesignThemes.Wpf.PackIconKind.PhoneIncoming;
                        break;
                    case LogView.eLogType.LEH:
                        result = MaterialDesignThemes.Wpf.PackIconKind.CalendarAlert;
                        break;
                    default:
                        result = MaterialDesignThemes.Wpf.PackIconKind.VectorPoint;
                        break;
                }
            }
            else
            {
                switch (value.ToString())
                {
                    case "Info":
                        result = MaterialDesignThemes.Wpf.PackIconKind.Information;
                        break;
                    case "Warn":
                        result = MaterialDesignThemes.Wpf.PackIconKind.Information;
                        break;
                    case "Error":
                        result = MaterialDesignThemes.Wpf.PackIconKind.Error;
                        break;
                    default:
                        result = MaterialDesignThemes.Wpf.PackIconKind.Information;
                        break;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ContentIconColorToLogLevelConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.Media.Brush result;
            LogView.eLogType type = LogView.eLogType.ENG;
            if (Enum.TryParse(value.ToString(), out type))
            {
                switch (type)
                {
                    case LogView.eLogType.FNC:
                        result = System.Windows.Media.Brushes.LightSeaGreen;
                        break;
                    case LogView.eLogType.XFR:
                        result = System.Windows.Media.Brushes.MediumBlue;
                        break;
                    case LogView.eLogType.PRC:
                        result = System.Windows.Media.Brushes.OrangeRed;
                        break;
                    case LogView.eLogType.ALM:
                        result = System.Windows.Media.Brushes.Red;
                        break;
                    case LogView.eLogType.CFG:
                        result = System.Windows.Media.Brushes.BlueViolet;
                        break;
                    case LogView.eLogType.COMM:
                        result = System.Windows.Media.Brushes.DodgerBlue;
                        break;
                    case LogView.eLogType.LEH:
                        result = System.Windows.Media.Brushes.Khaki;
                        break;
                    default:
                        result = System.Windows.Media.Brushes.LightSeaGreen;
                        break;
                }
            }
            else
            {
                switch (value.ToString())
                {
                    case "Info":
                        result = System.Windows.Media.Brushes.LightSeaGreen;
                        break;
                    case "Warn":
                        result = System.Windows.Media.Brushes.Orange;
                        break;
                    case "Error":
                        result = System.Windows.Media.Brushes.OrangeRed;
                        break;
                    default:
                        result = System.Windows.Media.Brushes.LightSeaGreen;
                        break;
                }
            }
            //string str = value.ToString();
            //switch (str)
            //{
            //    case "Info":
            //        result = System.Windows.Media.Brushes.LightSeaGreen;
            //        break;
            //    case "FNC":
            //        result = System.Windows.Media.Brushes.LightSeaGreen;
            //        break;
            //    case "PRC":
            //        result = System.Windows.Media.Brushes.OrangeRed;
            //        break;
            //    case "XFR":
            //        result = System.Windows.Media.Brushes.MediumBlue;
            //        break;
            //    case "Error":
            //        result = System.Windows.Media.Brushes.OrangeRed;
            //        break;
            //    default:
            //        result = System.Windows.Media.Brushes.LightSeaGreen;
            //        break;
            //}
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ContentToPathConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var ps = new PathSegmentCollection(4);
            ContentPresenter cp = (ContentPresenter)value;
            double h = cp.ActualHeight > 10 ? 1.4 * cp.ActualHeight : 10;
            double w = cp.ActualWidth > 10 ? 1.25 * cp.ActualWidth : 10;
            ps.Add(new LineSegment(new Point(1, 0.7 * h), true));
            ps.Add(new BezierSegment(new Point(1, 0.9 * h), new Point(0.1 * h, h), new Point(0.3 * h, h), true));
            ps.Add(new LineSegment(new Point(w, h), true));
            ps.Add(new BezierSegment(new Point(w + 0.6 * h, h), new Point(w + h, 0), new Point(w + h * 1.3, 0), true));
            PathFigure figure = new PathFigure(new Point(1, 0), ps, false);
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class LevelToIndentConverter : IValueConverter
    {
        private static readonly LevelToIndentConverter DefaultInstance = new LevelToIndentConverter();

        public static LevelToIndentConverter Default
        {
            get
            {
                return DefaultInstance;
            }
        }

        private const double IndentSize = 15.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness((int)value * IndentSize, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TextBoxToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int iValue = 0;
            double fValue = 0;
            if (Int32.TryParse(value.ToString(), out iValue))
            {
                return iValue;
            }
            else if (Double.TryParse(value.ToString(), out fValue))
            {
                return (int)fValue;
            }
            else
                return 0;
        }
    }

    public class TextBoxToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Double iValue = 0;
            if (Double.TryParse(value.ToString(), out iValue))
            {
                return iValue;
            }
            else
                return 0;
        }
    }

    public class TextBoxToLongConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long iValue = 0;
            if (long.TryParse(value.ToString(), out iValue))
            {
                return iValue;
            }
            else
                return 0;
        }
    }

    public class VisibleToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bvalue = (bool)value;
            Visibility visible = Visibility.Visible;
            switch (bvalue)
            {
                case true:
                    visible = Visibility.Visible;
                    break;
                case false:
                    visible = Visibility.Hidden;
                    break;
            }
            return visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class HeightToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bvalue = (bool)value;
            double visible = 0;

            switch (bvalue)
            {
                case true:
                    visible = 20;
                    break;
                case false:
                    visible = 0;
                    break;
            }
            return visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ConnectStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bvalue = (bool)value;
            string result = "Connect";
            switch (bvalue)
            {
                case true:
                    result = "DisConnect";
                    break;
                case false:
                    result = "Connect";
                    break;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ListViewContentsWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            ListBox listview = values[0] as ListBox;
            double width = (listview.ActualWidth - 10- ((listview.Items.Count-1)*2 + 2)*3) / listview.Items.Count;
            //Subtract 1, otherwise we could overflow to two rows.
            if (listview.Items.Count == 2)
                return (width <= 1) ? 0 : (width - 2);
            else
                return (width <= 1) ? 0 : (width - 1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TabSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            TabControl tabControl = values[0] as TabControl;
            double width = (int)(tabControl.ActualWidth - tabControl.Items.Count) / (double)tabControl.Items.Count;
            //Subtract 1, otherwise we could overflow to two rows.
            if (tabControl.Items.Count == 2)
                return (width <= 1) ? 0 : (width - 2);
            else
                return (width <= 1) ? 0 : (width-1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string EnumString;
            try
            {
                EnumString = Enum.GetName((value.GetType()), value);
                return EnumString;
            }
            catch
            {
                return string.Empty;
            }
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColumSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            TreeView tabControl = values[0] as TreeView;
            double col1width = 140;

            if (values[1] != null)
                col1width = (double)values[1];
            //double width = tabControl.ActualWidth / tabControl.Items.Count;
            //Subtract 1, otherwise we could overflow to two rows.
            return tabControl.ActualWidth - col1width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DictionaryItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {

                var dict = value as Dictionary<string, double>;
                if (dict != null)
                {
                    return dict[parameter as string];
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DictionaryItemKeysConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> result = new List<string>();
            var dict = value as Dictionary<string, double>;

            foreach (KeyValuePair<string, double> sKeyPos in dict)
            {
                result.Add(sKeyPos.Key);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DictionaryItemValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<double> result = new List<double>();
            var dict = value as Dictionary<string, double>;

            foreach (KeyValuePair<string, double> sKeyPos in dict)
            {
                result.Add(sKeyPos.Value);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterVisibleProgress : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int bvalue = (int)value;
            Visibility visible = Visibility.Visible;
            if (bvalue == 0 || bvalue == 100)
                visible = Visibility.Hidden;
            else
                visible = Visibility.Visible;
            return visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion
    }


    public class ZoomConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {  
            return System.Convert.ToInt32((double)value * 1000);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 1000;
        }
        #endregion
    }

    public class AjinAxisToIAxis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (AjinAxis)value;
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
}
