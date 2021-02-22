using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Root_AOP01_Packing
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public static BitmapImage Img_RTRA = new BitmapImage(new Uri("/Resource/RTR_A.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_RTRB = new BitmapImage(new Uri("/Resource/RTR_B.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_LoadportA = new BitmapImage(new Uri("/Resource/LOADPORT_A.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_LoadportB = new BitmapImage(new Uri("/Resource/LOADPORT_B.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_Unloadport = new BitmapImage(new Uri("/Resource/UNLOADPORT.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_Elevator = new BitmapImage(new Uri("/Resource/ElEVATOR.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_TapingModule = new BitmapImage(new Uri("/Resource/TAPING_MODULE.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_PackingLoader = new BitmapImage(new Uri("/Resource/PACKING_LOADER.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage Img_PackingStage = new BitmapImage(new Uri("/Resource/PACKING_STAGE.png", UriKind.RelativeOrAbsolute));
    }
}
