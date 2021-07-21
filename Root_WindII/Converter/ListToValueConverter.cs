using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WindII
{
    public class ListToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = (string)parameter;
            if (param == "No")
            {
                return ((RootTools.InfoWafer)value).m_nSlot + 1;
            }
            else if(param == "WaferID")
            {
                return ((RootTools.InfoWafer)value).p_sWaferID;
            }
            else if(param == "RecipeID")
            {
                return ((RootTools.InfoWafer)value).p_sRecipe;
            }
            else
            {
                return ((RootTools.InfoWafer)value).p_eState;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
