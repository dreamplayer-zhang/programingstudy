using RootTools.Control;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static RootTools.Control.Axis;

namespace Root_WINDII
{
    class AxisStateToLoadingOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Axis selectAxis = (Axis)value;
            if (selectAxis == null)
                return 0;

            if (selectAxis.p_eState == eState.Ready || selectAxis.p_eState == eState.Init)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
