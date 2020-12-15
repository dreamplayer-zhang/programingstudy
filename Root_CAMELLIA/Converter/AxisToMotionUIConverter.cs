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
            double canvasPos = 7000 * actPos / 2830000;
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
    public class AxisZToMotionUIConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double actPos = (double)value;
            double canvasPos = 9500 * actPos / 64000;
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
            double canvasPos = 9500 - (10000 * actPos / 103500);
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
