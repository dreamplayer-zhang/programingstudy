using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_VEGA_D
{
    public class NullToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 248, 255));
            }
            else
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 123, 104, 238));
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
