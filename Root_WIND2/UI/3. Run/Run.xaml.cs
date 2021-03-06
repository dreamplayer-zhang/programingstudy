using Root_EFEM.Module;
using Root_WIND2.Module;
using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_WIND2
{
    /// <summary>
    /// Run.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Run : UserControl
    {
        public Run()
        {
            InitializeComponent();
        }
       
    }
    public class ModuleStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RootTools.Module.ModuleBase.eState eState = (RootTools.Module.ModuleBase.eState)value;
            switch (eState)
            {
                //case RootTools.Module.ModuleBase.eState.Init: return Brushes.DimGray;
                //case RootTools.Module.ModuleBase.eState.Home: return Brushes.DimGray;
                //case RootTools.Module.ModuleBase.eState.Ready: return Brushes.LightYellow;
                case RootTools.Module.ModuleBase.eState.Run:
                    return Brushes.PaleGreen;
                case RootTools.Module.ModuleBase.eState.Error:
                    return Brushes.Red;
                default:
                    return SystemColors.ControlColorKey;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    public class PressureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double result = (int)value / 10.0;
            return result;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


        public class EQStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EQ.eState eState = (EQ.eState)value;
            switch (eState)
            {
                //case RootTools.Module.ModuleBase.eState.Init: return Brushes.DimGray;
                //case RootTools.Module.ModuleBase.eState.Home: return Brushes.DimGray;
                //case RootTools.Module.ModuleBase.eState.Ready: return Brushes.LightYellow;
                case EQ.eState.Run:
                    return Brushes.PaleGreen;
                case EQ.eState.Error:
                    return Brushes.Red;
                default:
                    return SystemColors.ControlColorKey;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ModuleRunStateBrushconverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleRunBase.eRunState state = (ModuleRunBase.eRunState)value;

            if (state == ModuleRunBase.eRunState.Done)
                return new SolidColorBrush(Colors.SteelBlue);
            if (state == ModuleRunBase.eRunState.Run)
                return new SolidColorBrush(Colors.LightSteelBlue);
            if (state == ModuleRunBase.eRunState.Error)
                return new SolidColorBrush(Colors.Crimson);
            else
                return new SolidColorBrush(Colors.AliceBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ModuleRunStepConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleRunBase module = (ModuleRunBase)value;

            //if (module.m_moduleBase.GetType() == typeof(VacuumPacker))
            //{
            //    if (module.GetType() == typeof(Run_Step))
            //        return module.p_id + "." + (module as Run_Step).m_eStep.ToString();
            //    else
            //        return module.p_id;
            //}
            //else
            //{
            return module.p_id;
            //}
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ModuleRunImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleRunBase module = (ModuleRunBase)value;
            var type = module.m_moduleBase.GetType();
            if (type == typeof(WTR_RND))
            {
                return App.Img_WTR;
            }
            else if (type == typeof(Loadport_RND))
            {
                return App.Img_LP;
            }
            else if (type == typeof(Aligner_ATI))
            {
                return App.Img_Aligner;
            }
            else if (type == typeof(BackSideVision))
            {
                return App.Img_BacksideVision;
            }
            else if (type == typeof(EdgeSideVision))
            {
                return App.Img_EdgeSideVision;
            }
            else
            {
                return App.Img_Vision;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ModuleRunStateVisibleConverter : IValueConverter
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
