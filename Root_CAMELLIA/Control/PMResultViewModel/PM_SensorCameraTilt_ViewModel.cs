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
using Met = Root_CAMELLIA.LibSR_Met;


namespace Root_CAMELLIA
{
    public class PM_SensorCameraTilt_ViewModel : ObservableObject
    {
        #region  VRS Camera View Image
        public PM_SensorCameraTilt_ViewModel()
        {
            ImageGrab();
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

        #endregion

        #region SensorCameraTilt Result

        PM_SensorCamera_Result m_pmSensorCameraResult = new PM_SensorCamera_Result();
        public PM_SensorCamera_Result p_pmSenserCamera
        {
            get
            {
                return m_pmSensorCameraResult;
            }
            set
            {
                SetProperty(ref m_pmSensorCameraResult, value);
            }
        }

        public class PM_SensorCamera_Result : ObservableObject
        {
            double m_SensorCameraAlign = 0.0;
            public double p_SensorCameraAlign
            {
                get
                {
                    return m_SensorCameraAlign;
                }
                set
                {
                    SetProperty(ref m_SensorCameraAlign, value);
                }
            }


        }
        #endregion
    }
}
