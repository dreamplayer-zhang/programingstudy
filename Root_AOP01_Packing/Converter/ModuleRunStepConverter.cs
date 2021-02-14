using Root_AOP01_Packing.Module;
using RootTools.Module;
using System;
using System.Globalization;
using System.Windows.Data;
using Root_EFEM.Module;
using static Root_AOP01_Packing.Module.VacuumPacker;

namespace Root_AOP01_Packing
{
    class ModuleRunStepConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleRunBase module = (ModuleRunBase)value;

            if (module.m_moduleBase.GetType() == typeof(VacuumPacker))
            {
                return module.p_id + "." + (module as Run_Step).m_eStep.ToString();
            }
            else
            {
                return module.p_id;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
