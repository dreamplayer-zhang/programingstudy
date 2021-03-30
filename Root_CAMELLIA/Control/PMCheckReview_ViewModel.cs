﻿using System;
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
    public class PMCheckReview_ViewModel : ObservableObject
    {
        #region
        RootViewer_ViewModel m_rootViewer = new RootViewer_ViewModel();
        public RootViewer_ViewModel p_rootViewer
        {
            get
            {
                return m_rootViewer;
            }
            set
            {
                SetProperty(ref m_rootViewer, value);
            }
        }
        #endregion
        public PMCheckReview_ViewModel()
        {
            Init();
        }

        public void Init()
        {
            ModuleCamellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;
            //pointListItem = new DataTable();
            p_DataTable.Columns.Add(new DataColumn("TIme"));
            p_DataTable.Columns.Add(new DataColumn("List"));
            p_DataTable.Columns.Add(new DataColumn("Result"));

            //p_rootViewer.p_ImgSource
            //p_rootViewer.p_ImageData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;
            //ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;
            //p_rootViewer.p_ImgSource = ImageHelper.GetBitmapSourceFromBitmap();
            ModuleCamellia.p_CamVRS.Captured += GetImage;
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

        private object lockObject = new object();
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

            DataTable m_DataTable = new DataTable();
        public DataTable p_DataTable
        {
            get
            {
                return m_DataTable;
            }
            set
            {
                SetProperty(ref m_DataTable, value);
            }
        }
        public void UpdatePMList ()
        {
            p_DataTable.Clear();
            DataRow row;
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

        public ICommand CmdReflectanceRepeatability
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PMReflectanceRepeatability();
                });
            }
        }
        public ICommand CmdThicknessRepeatbility
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PMThickenssRepeatability();
                });
            }
        }

        public ICommand cmdSensorStageAlign
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PMSensorStageAlign();
                });
            }
        }

        public ICommand CmdSensorCameraTilt
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PMSensorCameraTilt();
                });
            }
        }

        private void PMReflectanceRepeatability ()
        {
            //EQ.p_bStop = false;
            //if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            //{
            //    MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
            //    return;
            //}
            //Thread thread = new Thread(() =>
            //{
            //    Run_PMReflectance ReflectanceRepeatability = (Run_PMReflectance)ModuleCamellia.CloneModuleRun("PMTiltAlign");
            //    ReflectanceRepeatability.Run();
            //});
            //thread.Start();

            Mat mat = ImageHelper.ToMat(p_imageSource);
            //CvInvoke.Imshow("test", mat);
            //CvInvoke.WaitKey(0);
            //CvInvoke.DestroyAllWindows();

            //Image<Gray, byte> image = mat.ToImage<Gray, byte>();
            //Mat gray = new Mat();
            //gray = image.Mat;
            //CvInvoke.Sobel(gray, gray, Emgu.CV.CvEnum.DepthType.Cv8U, 1 ,1);

            //p_imageSource = ImageHelper.ToBitmapSource(gray.ToImage<Gray, byte>());

        }
        private void PMThickenssRepeatability ()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMThickness ThicknessRepeatability = (Run_PMThickness)ModuleCamellia.CloneModuleRun("PMTiltAlign");
                ThicknessRepeatability.Run();
            });
            thread.Start();
        }
        private void PMSensorStageAlign ()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMSensorStageAlign SensorStageAlign = (Run_PMSensorStageAlign)ModuleCamellia.CloneModuleRun("PMTiltAlign");
                SensorStageAlign.Run();
            });
            thread.Start();
        }
        private void PMSensorCameraTilt()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMSensorCameraTilt SensorCameraTilt= (Run_PMSensorCameraTilt)ModuleCamellia.CloneModuleRun("PMTiltAlign");
                SensorCameraTilt.Run();
            });
            thread.Start();
        }
    }
}
