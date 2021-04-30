using RootTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Root_CAMELLIA
{
    public class OpticLampStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case LibSR_Met.Nanoview.CheckLampState.Error_Controller_Power_OFF:
                case LibSR_Met.Nanoview.CheckLampState.Error_Lamp_Switch_OFF:
                case LibSR_Met.Nanoview.CheckLampState.Error_Lamp_Temperature_High:
                    return Brushes.Crimson;
                case LibSR_Met.Nanoview.CheckLampState.OFF:
                    return Brushes.LightGray;
                case LibSR_Met.Nanoview.CheckLampState.ON:
                    return Brushes.ForestGreen;
                case LibSR_Met.Nanoview.CheckLampState.WarmUP:
                    return Brushes.Orange;
                case LibSR_Met.Nanoview.CheckLampState.SignalError:
                    return Brushes.Purple;
                default:
                    return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OpticLampTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int time = System.Convert.ToInt32(value);
            string LampPMCheck = "";
            if (time >= 9500 && time < 10000)
            {
                LampPMCheck = 10000 - time + "Hours Left Until PM";
            }
            else if (time > 10000)
            {
                LampPMCheck = "Check Lamp";
            }
            else
            {
                LampPMCheck = "";
            }
            return LampPMCheck;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OpticLampStatusEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = Enum.GetName(typeof(LibSR_Met.Nanoview.CheckLampState), value);
            if (name.Contains("Error"))
            {
                name = "Error";
            }
            return name;
            //throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
