using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_CAMELLIA
{
    public class AxisXToMotionUIConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double actPos = (double)value;
            double canvasPos = (7000 * actPos / 5290000);
            // actPos : maxPos = canvasleft : 10000
            if (value == null)
            {
                return 0.0;
            }

            return canvasPos;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public class AxisYToMotionUIConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double actPos = (double)value;
            double canvasPos = 7000 * actPos / 2938590; // 최대, 최소 구해야함
            if (value == null)
            {
                return 0.0;
            }

            return 7000-canvasPos;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public class AxisZToMotionUIConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //-78803
            //655730
            double actPos = (double)value;
            double canvasPos = 9500 * actPos / 655730;
            if (value == null)
            {
                return 0.0;
            }

            return canvasPos;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public class AxisLifterToMotionUIConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double actPos = (double)value;
            double canvasPos = 9500 - (9500 * actPos / 103948);
            if (value == null)
            {
                return 0.0;
            }

            return canvasPos;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
