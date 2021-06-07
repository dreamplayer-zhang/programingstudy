using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Root_WIND2
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public static BitmapImage Img_Vision = new BitmapImage(new Uri("/Resources/VISION.JPG", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_WTR = new BitmapImage(new Uri("/Resources/WTR.JPG", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_LP = new BitmapImage(new Uri("/Resources/LOADPORT.JPG", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_Aligner = new BitmapImage(new Uri("/Resources/ALIGNER.JPG", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_BacksideVision = new BitmapImage(new Uri("/Resources/BACKSIDE ISO VIEW.JPG", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_EdgeSideVision = new BitmapImage(new Uri("/Resources/EDGESIDE.JPG", UriKind.RelativeOrAbsolute));
    }
}
