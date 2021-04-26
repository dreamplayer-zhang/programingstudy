using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Root_CAMELLIA
{
    public class ValueToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // SolidColorBrush brush = new SolidColorBrush();
            try
            {
                double from = (double)values[0];
                double to = (double)values[1];
                double val = (double)values[2];
                if ((double)values[0] > (double)values[2])
                {
                    return Brushes.Gold;
                }
                else if (from + ((to - from) * 0.1) > val)
                {
                    return Brushes.Orange;
                }
                else if ((double)values[1] < (double)values[2])
                {
                    return Brushes.Red;
                }
                else if (from + ((to - from) * 0.9) < (double)values[2])
                {
                    return new SolidColorBrush(Color.FromRgb(255, 70, 0));
                }
                else
                {
                    return Brushes.Black;
                }
            }
            catch(Exception e)
            {

            }
            return Brushes.Black;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueToProgressColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((int)value == 100)
            {
                return Brushes.Blue;
            }
            return Brushes.ForestGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OpticLampStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
