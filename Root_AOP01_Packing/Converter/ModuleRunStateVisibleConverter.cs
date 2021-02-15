using RootTools.Module;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace Root_AOP01_Packing
{
    class ModuleRunStateVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleRunBase.eRunState state = (ModuleRunBase.eRunState)value;
            if ((string)parameter == "Opacity")
            {
                if (state == ModuleRunBase.eRunState.Run)
                    return 0.5;
                else
                    return 1;
            }
            else
            {
                if (state == ModuleRunBase.eRunState.Run)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
