using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WINDII
{
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
                return null;
        }
    }

    public class DoubleToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string res = ((double)value).ToString("0.##");
            return res + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
