using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_CAMELLIA
{
    public class MeasurePointToStageUIConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double res = (double)value;
            string Axis = parameter.ToString();
            if(Axis == "X")
            {
                res += (BaseDefine.CanvasWidth / 2);
            }
            else
            {
                res += (BaseDefine.CanvasHeight / 2);
            }
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
