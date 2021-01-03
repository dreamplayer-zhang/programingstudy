using RootTools.Control;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_CAMELLIA
{
    public class AxisToStageUIConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double center = (double)values[0];
                double current = (double)values[1];
                Axis axis = (Axis)values[2];
                double res;
                double ratio = (int)(BaseDefine.CanvasWidth / BaseDefine.ViewSize);
                if (axis.p_id == "Camellia.StageXYX")
                {
                    res = (BaseDefine.CanvasWidth / 2) - ((center - current) / (10000 / ratio)) - (double.Parse((string)parameter) / 2);
                }
                else
                {
                    res = (BaseDefine.CanvasHeight / 2) + ((center - current) / (10000 / ratio)) - (double.Parse((string)parameter) / 2);
                }
                return res;
            }
            catch (Exception)
            {
            }
            return values;
         
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
