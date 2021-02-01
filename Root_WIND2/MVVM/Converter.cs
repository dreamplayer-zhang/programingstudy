using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        public object Convert(object value,  Type targetType, object parameter, CultureInfo culture)
        {
           
            var a = ((Color)value).A;
            var r = ((Color)value).R;
            var g = ((Color)value).G;
            var b = ((Color)value).B;
            //var a = ((System.Drawing.Color)_color).A;
            //var r = ((System.Drawing.Color)_color).R;
            //var g = ((System.Drawing.Color)_color).G;
            //var b = ((System.Drawing.Color)_color).B;
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
    public class IndentConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return new Thickness(0);

            return new Thickness(Length * GetDepth(item), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public int GetDepth(TreeViewItem item)
        {
            TreeViewItem parent;
            while ((parent = GetParent(item)) != null)
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        private TreeViewItem GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
    }

    public class CollectionCountToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((int)value == 0) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility result = (Visibility)Enum.Parse(typeof(Visibility), value.ToString(), true);

            return result == Visibility.Visible ? true : false;
        }
    }
}
