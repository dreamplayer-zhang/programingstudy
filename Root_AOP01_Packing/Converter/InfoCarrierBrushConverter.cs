using RootTools;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Root_AOP01_Packing
{
    class InfoCarrierBrushConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            InfoCarrier.eState state = (InfoCarrier.eState)value;
            if ((string)parameter == "Load")
            {
                if (state == InfoCarrier.eState.Dock)
                    return new SolidColorBrush(Colors.SteelBlue);
                else
                    return new SolidColorBrush(Colors.Transparent);              
            }
            else
            {
                if (state == InfoCarrier.eState.Placed)
                    return new SolidColorBrush(Colors.SteelBlue);
                else
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
