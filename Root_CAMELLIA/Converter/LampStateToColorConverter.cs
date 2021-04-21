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
    class LampStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(EQ.p_eState == EQ.eState.Error)
            {
                if((string)parameter == "top")
                {
                    return new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                    
                }
                else if((string)parameter == "middle")
                {
                    return new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                }
                else
                {
                    return Brushes.Crimson;
                }
            }
            else if(EQ.p_eState == EQ.eState.Run)
            {
                if ((string)parameter == "top")
                {
                    return Brushes.ForestGreen;

                }
                else if ((string)parameter == "middle")
                {
                    return new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                }
            }
            else
            {
                if ((string)parameter == "top")
                {
                    return new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));

                }
                else if ((string)parameter == "middle")
                {
                    return Brushes.Gold;   
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
