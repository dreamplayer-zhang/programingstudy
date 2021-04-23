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
using System.Drawing;
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

            p_imageSource = ImageHelper.ToBitmapSource(img);
    
        }
        public void CalImage()
        {
            OpenCvSharp.CPlusPlus.Mat imgMat = OpenCvSharp.CPlusPlus.Cv2.ImRead("C:\\Users\\ATI\\Desktop\\Met DS\\new20210324\\x1821860_y157840_z476453.png", OpenCvSharp.LoadMode.GrayScale);
            Bitmap StageImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imgMat);
            //open cv 로 받은 bitmap 이미지 bitmapSource로 변환하기
            IntPtr hbitmap = StageImage.GetHbitmap();
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
            BitmapSource CalBitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
            CalBitmapSource.Freeze();

            p_imageSource = CalBitmapSource;
        }
        public void CalImage(OpenCvSharp.CPlusPlus.Mat imgMat)
        {
            //OpenCvSharp.CPlusPlus.Mat imgMat = OpenCvSharp.CPlusPlus.Cv2.ImRead("D:\\Nano-View\\SW\\2021.02.15 PM 시작\\new20210324\\x1821860_y157840_z476453.png", OpenCvSharp.LoadMode.GrayScale);
            Bitmap StageImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imgMat);
            //open cv 로 받은 bitmap 이미지 bitmapSource로 변환하기
            IntPtr hbitmap = StageImage.GetHbitmap();
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
            BitmapSource CalBitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
            CalBitmapSource.Freeze();

            p_imageSource = CalBitmapSource;
        }
        //이전 Grab 한 이미지 받아서 이미지 처리한 이미지 다시 뿌리기
        public void CalImage(string GrabImageFullPath)
        {
            OpenCvSharp.CPlusPlus.Mat imgMat = OpenCvSharp.CPlusPlus.Cv2.ImRead(GrabImageFullPath, OpenCvSharp.LoadMode.GrayScale);
            Bitmap StageImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imgMat);
            //open cv 로 받은 bitmap 이미지 bitmapSource로 변환하기
            IntPtr hbitmap = StageImage.GetHbitmap();
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
            BitmapSource CalBitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
            CalBitmapSource.Freeze();

            p_imageSource = CalBitmapSource;
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

        #region Result Offset Value
        PM_SensorHole_Result m_pmSensorHoleResult = new PM_SensorHole_Result();
        public PM_SensorHole_Result p_pmSensorHoleAlign
        {
            get
            {
                return m_pmSensorHoleResult;
            }
            set
            {
                SetProperty(ref m_pmSensorHoleResult, value);
            }
        }

        public class PM_SensorHole_Result : ObservableObject
        {
            double m_HoleOffsetX = 0.0;
            double m_HoleOffsetY = 0.0;
            double m_SensorOffsetX = 0.0;
            double m_SensorOffsetY = 0.0;
            double m_TotalOffsetX = 0.0;
            double m_TotalOffsetY = 0.0;

            public double p_HoleOffsetX { get { return m_HoleOffsetX; } set { SetProperty(ref m_HoleOffsetX, value); } }
            public double p_HoleOffsetY { get { return m_HoleOffsetY; } set { SetProperty(ref m_HoleOffsetY, value); } }
            public double p_SensorOffsetX { get { return m_SensorOffsetX; } set { SetProperty(ref m_SensorOffsetX, value); } }
            public double p_SensorOffsetY { get { return m_SensorOffsetY; } set { SetProperty(ref m_SensorOffsetY, value); } }
            public double p_TotalOffsetX { get { return m_TotalOffsetX; } set { SetProperty(ref m_TotalOffsetX, value); } }
            public double p_TotalOffsetY { get { return m_TotalOffsetY; } set { SetProperty(ref m_TotalOffsetY, value); } }

        }
        #endregion
    }
}
