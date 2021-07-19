using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Root_WindII
{
    public class PathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
            //if (value == null)
            //{
            //    return null;
            //}
            //object res;
            //if (parameter != null && parameter.ToString() == "List")
            //{
            //    //List<string> list = new List<string>();
            //    //foreach (var str in (List<ModelData.LayerData.PathEntity>)value)
            //    //{
            //    //    string path = Path.GetFileNameWithoutExtension(str.FullPath);
            //    //    list.Add(path);
            //    //}
            //    //res = list;
            //}
            //else
            //{
            //    string name = Path.GetFileNameWithoutExtension(value.ToString());
            //    res = name;
            //}
            //return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
            //throw new NotImplementedException();
        }
    }
}
