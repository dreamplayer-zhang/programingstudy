using RootTools.Module;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Root_AOP01_Packing
{
    class ModuleStateBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleBase.eState state = (ModuleBase.eState)value;

            if(state == ModuleBase.eState.Error)
                return new SolidColorBrush(Colors.Crimson);
            else
                return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
