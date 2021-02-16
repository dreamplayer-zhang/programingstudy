using Root_AOP01_Packing.Module;
using RootTools.Module;
using System;
using System.Globalization;
using System.Windows.Data;
using Root_EFEM.Module;
using static Root_AOP01_Packing.Module.VacuumPacker;

namespace Root_AOP01_Packing
{
    class ModuleRunImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleRunBase module = (ModuleRunBase)value;

            if (module.m_moduleBase.GetType() == typeof(TapePacker))
                return App.Img_TapingModule;
            if (module.m_moduleBase.GetType() == typeof(IndividualElevator))
                return App.Img_Elevator;
            if (module.m_moduleBase.GetType() == typeof(Loadport_AOP))
                return App.Img_LoadportB;
            if (module.m_moduleBase.GetType() == typeof(Loadport_Cymechs))
                return App.Img_LoadportA;
            if (module.m_moduleBase.GetType() == typeof(Unloadport_AOP))
                return App.Img_Unloadport;
            if (module.m_moduleBase.GetType() == typeof(WTR_RND))
            {
                if (module.m_moduleBase.p_id.Contains("_A"))
                    return App.Img_RTRA;
                else
                    return App.Img_RTRB;
            }
            if (module.m_moduleBase.GetType() == typeof(VacuumPacker))
            {
                if (module.GetType() == typeof(Run_Step))
                {
                    Run_Step step = (Run_Step)module;
                    if (step.m_eStep == eStep.InputVacArm ||
                        step.m_eStep == eStep.InsertCase ||
                        step.m_eStep == eStep.PushToLoader ||
                        step.m_eStep == eStep.Unload)
                        return App.Img_PackingLoader;
                    else
                        return App.Img_PackingStage;
                }

                return App.Img_PackingStage;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
