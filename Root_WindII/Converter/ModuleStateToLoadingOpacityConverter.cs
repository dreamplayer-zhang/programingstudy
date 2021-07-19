using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WindII
{
    class ModuleStateToLoadingOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ModuleBase.eState.Run == (ModuleBase.eState)value || RootTools.EQ.m_EQ.p_eState == RootTools.EQ.eState.Run)
            {
                return 1.0;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleStateToLoadingEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ModuleBase.eState.Run == (ModuleBase.eState)value || RootTools.EQ.p_eState == RootTools.EQ.eState.Run)
            {
                return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
