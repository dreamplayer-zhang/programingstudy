using System;
using System.Windows;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using System.Windows.Input;
using System.Threading;
using Root_CAMELLIA.Module;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Media.Imaging;


namespace Root_CAMELLIA
{
    public class PM_SensorHoleOffset_ViewModel : ObservableObject
    {
        public PM_SensorHoleOffset_ViewModel()
        {
            ImageGrab();
        }

        BitmapSource m_imageSource;
        public BitmapSource p_imageSource
        {
            get
            {
                return m_imageSource;
            }
            set
            {
                SetProperty(ref m_imageSource, value);
            }
        }


        public void ImageGrab()
        {
            ModuleCamellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;
            ModuleCamellia.p_CamVRS.Captured += GetImage;
        }
        private void GetImage(object obj, EventArgs e)
        {
            Thread.Sleep(100);
            RootTools.Camera.BaslerPylon.Camera_Basler p_CamVRS = ModuleCamellia.p_CamVRS;
            Mat mat = new Mat(new System.Drawing.Size(p_CamVRS.GetRoiSize().X, p_CamVRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, p_CamVRS.p_ImageViewer.p_ImageData.GetPtr(), (int)p_CamVRS.p_ImageViewer.p_ImageData.p_Stride * 3);
            Image<Bgra, byte> img = mat.ToImage<Bgra, byte>();

            //CvInvoke.Imshow("aa",img.Mat);
            //CvInvoke.WaitKey(0);
            //CvInvoke.DestroyAllWindows();
            //p_rootViewer.p_ImageData = new ImageData(p_CamVRS.p_ImageViewer.p_ImageData);
            //lock (lockObject)
            //{

            p_imageSource = ImageHelper.ToBitmapSource(img);
            //}
            //p_rootViewer.SetImageSource();
        }

        private Module_Camellia moduleCamellia;
        public Module_Camellia ModuleCamellia
        {
            get
            {
                return moduleCamellia;
            }
            set
            {
                SetProperty(ref moduleCamellia, value);
            }
        }
    }
}
