using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WindII
{
    class HumidityToGradientOffsetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double offset = 4;
            try
            {
                double upper = (double)values[2];
                double lower = (double)values[1];
                int val = (int)values[0];

                offset = 4 - (4 * (val / (upper - lower)));
                if (offset < 0)
                {
                    offset = 0;
                }
                return offset;
            }
            catch
            {
                return 0;
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
