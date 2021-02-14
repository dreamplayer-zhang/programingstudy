using RootTools.Module;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Root_AOP01_Packing
{
    class ModuleRunStateBrushconverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleRunBase.eRunState state = (ModuleRunBase.eRunState)value;

            if (state == ModuleRunBase.eRunState.Done)
                return new SolidColorBrush(Colors.SteelBlue);
            if (state == ModuleRunBase.eRunState.Run)
                return new SolidColorBrush(Colors.LightSteelBlue);
            else
                return new SolidColorBrush(Colors.AliceBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
