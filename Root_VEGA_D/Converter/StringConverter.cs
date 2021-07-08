using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Root_VEGA_D
{
    class StringConverter
    {
    }

    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string; if (parameterString == null) { return DependencyProperty.UnsetValue; }
            if (Enum.IsDefined(value.GetType(), value) == false) { return DependencyProperty.UnsetValue; }
            object parameterValue = Enum.Parse(value.GetType(), parameterString); return parameterValue.Equals(value);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string; if (parameterString == null) { return DependencyProperty.UnsetValue; }
            return Enum.Parse(targetType, parameterString);
        }
    }

    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string; if (parameterString == null) { return Visibility.Collapsed; }
            if (Enum.IsDefined(value.GetType(), value) == false) { return Visibility.Collapsed; }
            object parameterValue = Enum.Parse(value.GetType(), parameterString);
            return parameterValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string; if (parameterString == null) { return DependencyProperty.UnsetValue; }
            return Enum.Parse(targetType, parameterString);
        }
    }
}
