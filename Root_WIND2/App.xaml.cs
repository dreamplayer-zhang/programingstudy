using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        public App()
        {
            this.Dispatcher.UnhandledException += UnExceptEventLogger;
        }

        public void UnExceptEventLogger(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string strTime = DateTime.Now.ToString("yyyyMMdd");
            string filePath = @"C:\Log\" + "ExceptionLog_" + strTime + ".txt";
            FileInfo file = new FileInfo(filePath);
            StreamWriter sw;

            if (!file.Exists)
            {
                sw = file.CreateText();
            }
            else
            {
                sw = file.AppendText();
            }

            sw.WriteLine(string.Format("[ {0:yyyy-MM-dd hh:mm s} ]" + System.Diagnostics.Process.GetCurrentProcess().ProcessName, DateTime.Now));
            sw.WriteLine("Module : " + e.Exception.Source);
            sw.WriteLine("Message : " + e.Exception.Message);
            sw.WriteLine(e.Exception.StackTrace.ToString());
            sw.WriteLine("\n\n");
            sw.Flush();
            sw.Close();
        }
    }
}