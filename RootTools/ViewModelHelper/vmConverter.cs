using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using RootTools;
using System.Globalization;
using RootTools.Control.Ajin;
using RootTools.Control;

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

    public class InverseVisibleToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bvalue = !(bool)value;
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

    public class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            Visibility v = (Visibility)value;
            if (v == Visibility.Visible) return Visibility.Collapsed;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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

    public class BoolToEnableColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue == true) return Brushes.Green;
            else return Brushes.Crimson;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToEnableStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue == true) return "Enable";
            else return "Disable";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToRunColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue == true) return Brushes.Yellow;
            else return Brushes.DarkGray;
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToRunColor_2_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue == true) return Brushes.SeaGreen;
            else return Brushes.DimGray;
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DoubleToPercentStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double dValue = (double)value;
            return dValue.ToString("0.0") + "%";
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
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

    public class PositionScalingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // variable
                if (values[0] == null)
                {
                    return 0.0f;
                }
                //AjinAxis axis = (AjinAxis)values[0];
                Axis axis = (Axis)values[0];
                double dStageLength = (double)values[1];
                double dMinusLimit = axis.GetPosValue(Axis.ePosition.SWLimit_Minus);
                double dPlusLimit = axis.GetPosValue(Axis.ePosition.SWLimit_Plus);
                double dActualPos = axis.p_posActual;
                double dScaled = 0.0;
                double dControlLength = 0.0;    // Control별 사이즈, 스테이지 영역을 넘어가지 않게 하기 위해 빼주는 값
                string strAxis = (string)parameter;
                string strCaller = (string)values[3];

                // implement
                switch(strCaller)
                {
                    case "PatternVision":
                        if (strAxis == "AxisX")
                        {
                            dControlLength = 50.0;
                            dScaled = PositionScaling(dActualPos, dMinusLimit, dPlusLimit, dStageLength - dControlLength, 0.0);
                        }
                        else if (strAxis == "AxisY")
                        {
                            dControlLength = 100.0;
                            dScaled = PositionScaling(dActualPos, dMinusLimit, dPlusLimit, dStageLength - dControlLength, 0.0);
                        }
                        else if (strAxis == "AxisZ")
                        {
                            dControlLength = 30.0;
                            dScaled = PositionScaling(dActualPos, dMinusLimit, dPlusLimit, 0.0, dStageLength - dControlLength);
                        }
                        break;
                    case "SideVision":
                        if (strAxis == "AxisX")
                            dControlLength = 150.0;
                        else if (strAxis == "AxisY")
                            dControlLength = 100.0;
                        else if (strAxis == "AxisZ")
                            dControlLength = 30.0;
                        else if (strAxis == "AxisTheta")
                        {
                            dMinusLimit = -180000.0;
                            dPlusLimit = 180000.0;
                            dStageLength = 360.0;
                            dControlLength = 0.0;
                        }
                        dScaled = PositionScaling(dActualPos, dMinusLimit, dPlusLimit, 0.0, dStageLength - dControlLength);
                        break;
                }

                return dScaled;
            }
            catch
            {
                return 0.0f;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private double PositionScaling(double dValue, double dMinValue, double dMaxValue, double dMinScaleValue, double dMaxScaleValue)
        {
            double dScaled = dMinScaleValue + (dValue - dMinValue) / (dMaxValue - dMinValue) * (dMaxScaleValue - dMinScaleValue);
            return dScaled;
        }
    }

    //public class AjinModuleToPackIconConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        PackIconKind result = PackIconKind.AlphaXBox;
    //        string str = value.ToString();
    //        AXT_MODULE type = AXT_MODULE.AXT_SIO_DI32;
    //        if (Enum.TryParse(value.ToString(), out type))
    //        {
    //            switch (type)
    //            {
    //                case AXT_MODULE.AXT_SIO_DI32:
    //                case AXT_MODULE.AXT_SIO_RDI32MLIII:
    //                case AXT_MODULE.AXT_SIO_RDI32PMLIII:
    //                    result = PackIconKind.AlphaIBox;
    //                    break;
    //                case AXT_MODULE.AXT_SIO_DO32P:
    //                case AXT_MODULE.AXT_SIO_RDO32MLIII:
    //                case AXT_MODULE.AXT_SIO_RDO32PMLIII:
    //                    result = PackIconKind.AlphaOBox;
    //                    break;
    //                case AXT_MODULE.AXT_SIO_DB32P:
    //                case AXT_MODULE.AXT_SIO_RDB32MLIII:
    //                case AXT_MODULE.AXT_SIO_RDB32PMLIII:
    //                    result = PackIconKind.AlphaBBox;
    //                    break;
    //            }
    //        }
    //        return result;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class DataContextToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility result = Visibility.Visible;
            if (value == null)
            {
                result = Visibility.Hidden;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class SequenceStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RootTools.Module.ModuleRunBase.eRunState eState = (RootTools.Module.ModuleRunBase.eRunState)value;
            switch(eState)
            {
                case RootTools.Module.ModuleRunBase.eRunState.Done: return Brushes.SeaGreen;
                case RootTools.Module.ModuleRunBase.eRunState.Ready: return Brushes.DimGray;
                case RootTools.Module.ModuleRunBase.eRunState.Run: return Brushes.Yellow;
                default: return Brushes.DimGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    class VisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0].ToString().Equals("Admin") && values[1].ToString().Equals("Pass"))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
