using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Root_WIND2
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CollectionViewSource itemSource = parameter as CollectionViewSource;
            IEnumerable<object> enumerItems = itemSource.Source as IEnumerable<object>;
            List<object> listItems = enumerItems.ToList();


            return listItems.IndexOf(value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
    public class StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var a = ((System.Drawing.Color)value).A;
            var r = ((System.Drawing.Color)value).R;
            var g = ((System.Drawing.Color)value).G;
            var b = ((System.Drawing.Color)value).B;
            Color color = Color.FromArgb(a, r, g, b);

            return new SolidColorBrush(color);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
    public class PropertyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            //if (value == null || value.GetType() != typeof(InspectionMethod))
            //    return null;

            //InspectionMethod item = value as InspectionMethod;
            //switch (item.p_inspMode)
            //{
            //    case InspectionMode.Surface:
            //        {
            //            return item.m_Surface;
            //        }
            //    case InspectionMode.D2D:
            //        {
            //            return item.m_D2D;
            //        }

            //}
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


}
