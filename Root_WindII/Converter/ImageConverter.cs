using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Root_WindII
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if(value == null)
            {
                return null;
            }
            string ext = Path.GetExtension(value.ToString());
            string res = null;
            if (ext == ".ref") ///////// ref 파일 일때 
            {
                res = "..\\Resource\\icon_ref.png";
            }
            else if (ext == ".dis") ////// dis 파일 일때 
            {
                res = "..\\Resource\\icon_dis.png";
            }

            //System.Windows.Controls.Image icoImage = new System.Windows.Controls.Image();
            //icoImage.Source = new BitmapImage(new Uri(res, UriKind.Relative));
            //icoImage.Width = 40;
            //icoImage.Height = 40;


            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
