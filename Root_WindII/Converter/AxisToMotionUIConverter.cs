using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WindII
{
    public class AxisToMotionUIMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
            {
                return 0.0f;
            }
            double value = System.Convert.ToDouble(values[0]);
            double plus = ((RootTools.Control.Axis)values[1]).GetPosValue(RootTools.Control.Axis.ePosition.SWLimit_Plus);
            double minus = ((RootTools.Control.Axis)values[1]).GetPosValue(RootTools.Control.Axis.ePosition.SWLimit_Minus);

            string param = (string)parameter;
            double canvasPos = 0.0;
            double totalPos = plus - minus;
            if (param == "AxisX")
            {
                canvasPos = 7000 * value / totalPos;
            }
            else if(param == "AxisY")
            {
                canvasPos = 7000 - (7000 * value / totalPos);
                if (double.IsNaN(canvasPos))
                {
                    canvasPos = 7000;
                }
            }
            else if(param == "AxisZ")
            {
                canvasPos = 9500 * value / totalPos;
            }
            else if(param == "AxisLifter")
            {
                canvasPos = 9500 - (9500 * value / totalPos);
                if (double.IsNaN(canvasPos))
                {
                    canvasPos = 9500;
                }
            }
            
            return canvasPos;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    //public class AxisXToMotionUIConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        double actPos = (double)value;
    //        double canvasPos = (7000 * actPos / 5290000); // 여기 SOFTLIMIT으로 변경필요
    //        // actPos : maxPos = canvasleft : 10000
    //        if (value == null)
    //        {
    //            return 0.0;
    //        }

    //        return canvasPos;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}

    //public class AxisXToMotionUIMultiConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {

    //        if (values[0] == null || values[1] == null)
    //        {
    //            return 0.0;
    //        }
    //        double value = System.Convert.ToDouble(values[0]);
    //        double plus = ((RootTools.Control.Axis)values[1]).GetPosValue(RootTools.Control.Axis.ePosition.SWLimit_Plus);
    //        double minus = ((RootTools.Control.Axis)values[1]).GetPosValue(RootTools.Control.Axis.ePosition.SWLimit_Minus);

    //        double canvasPos = (7000 * value / (plus - minus)); // 여기 SOFTLIMIT으로 변경필요


    //        return canvasPos;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    //public class AxisYToMotionUIConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        double actPos = (double)value;
    //        double canvasPos = 7000 * actPos / 2938590; // 최대, 최소 구해야함
    //        if (value == null)
    //        {
    //            return 0.0;
    //        }

    //        return 7000-canvasPos;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}

    //public class AxisZToMotionUIConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        //-78803
    //        //655730
    //        double actPos = (double)value;
    //        double canvasPos = 9500 * actPos / 655730;
    //        if (value == null)
    //        {
    //            return 0.0;
    //        }

    //        return canvasPos;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}
    //public class AxisLifterToMotionUIConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        double actPos = (double)value;
    //        double canvasPos = 9500 - (9500 * actPos / 103948);
    //        if (value == null)
    //        {
    //            return 0.0;
    //        }

    //        return canvasPos;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}
}
