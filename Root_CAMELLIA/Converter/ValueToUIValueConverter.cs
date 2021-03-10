using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_CAMELLIA
{
    class ValueToUIValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           double val = double.Parse(value.ToString());
           if((string)parameter == "Humidity")
            {
                return string.Format("{0} %", val / 10);
            }
            else
            {
                return string.Format("{0} °C", val / 10);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
