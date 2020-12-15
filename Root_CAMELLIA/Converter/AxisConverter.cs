using Root_CAMELLIA.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_CAMELLIA
{
    public class AxisConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //Module.Module_Camellia m_camellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;
            //Module.Module_Camellia.Run_Measure mea = (Module.Module_Camellia.Run_Measure)m_camellia.CloneModuleRun("Measure");
            double pulse = (double)values[1];
            DataManager dm = DataManager.Instance;

            double res = 0;
           // double test = (double)values[2];
            if ((string)parameter == "AxisX")
            {
                double CenterX = dm.m_waferCentering.m_ptStageCenter.X;
                res = pulse - CenterX;
            }
            else if((string)parameter == "AxisY")
            {
                double CenterY = dm.m_waferCentering.m_ptStageCenter.Y;
                res = pulse - CenterY;
            }

            res /= 1000;

            return res.ToString("0.###") + "mm";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
