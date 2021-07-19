using RootTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WindII
{
    class ValueToUIValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           double val = double.Parse(value.ToString());
           if((string)parameter == "Humidity")
            {
                return string.Format("{0} %", val / 10);
            }
            else if ((string)parameter == "Fan")
            {
                return string.Format("{0} Pa", val); 
            }
            else if((string)parameter == "RPM")
            {
                return string.Format("{0} RPM", val);
            }
            else
            {
                return string.Format("{0} °C", val / 10);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = value.GetType();
            if(type == typeof(RootTools.Gem.XGem.XGem.eControl))
            {
                string val = Enum.GetName(type, value);
                if (val == "ONLINEREMOTE")
                {
                    return "Online Remote";
                }
                else if (val == "LOCAL")
                {
                    return "Online Local";
                }
                else
                {
                    return "Offline";
                }
            }
            return Enum.GetName(type, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
