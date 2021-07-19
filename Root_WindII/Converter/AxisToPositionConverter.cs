using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WINDII
{
    public class AxisToPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double pulse = (double)values[1];
                double center = (double)values[2];
                double res =  center - pulse;

                res /= 10000;
                return res.ToString("0.###") + "mm";
            }
            catch (Exception)
            {
            }
            return values;
         

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
