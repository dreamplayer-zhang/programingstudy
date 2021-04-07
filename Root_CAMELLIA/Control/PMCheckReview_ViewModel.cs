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
            PageInit();
        }

        object m_page;
        public object p_page
        {
            get
            {
                return m_page;
            }
            set
            {
                SetProperty(ref m_page, value);
            }
        }

        #region Page ViewModel
        PM_Reflectance_ViewModel m_pmReflectance_VM;
        PM_SensorCameraTilt_ViewModel m_pmSensorTilt_VM;
        PM_SensorHoleOffset_ViewModel m_pmSensorHoleOffset_VM;
        PM_Thickness_ViewModel m_pmThickness_VM;
        #endregion

        public void PageInit()
        {
            m_pmReflectance_VM = new PM_Reflectance_ViewModel();
            m_pmSensorTilt_VM = new PM_SensorCameraTilt_ViewModel();
            m_pmSensorHoleOffset_VM = new PM_SensorHoleOffset_ViewModel();
            m_pmThickness_VM = new PM_Thickness_ViewModel();
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

        public void UpdatePMList()
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
                    p_page = m_pmReflectance_VM;
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
                    p_page = m_pmThickness_VM;
                    PMThickenssRepeatability();
                });
            }
        }

        public ICommand CmdSensorStageAlign
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_page = m_pmSensorHoleOffset_VM;
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
                    p_page = m_pmSensorTilt_VM;
                    PMSensorCameraTilt();

                });
            }
        }

        private void PMReflectanceRepeatability()
        {
            
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMReflectance ReflectanceRepeatability = (Run_PMReflectance)ModuleCamellia.CloneModuleRun("PMReflectance");
                ModuleCamellia.StartRun(ReflectanceRepeatability);
                //ReflectanceRepeatability.Run();
            });
            thread.Start();
        }
        private void PMThickenssRepeatability()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMThickness ThicknessRepeatability = (Run_PMThickness)ModuleCamellia.CloneModuleRun("PMThickness");
                ModuleCamellia.StartRun(ThicknessRepeatability);
            });
            thread.Start();
        }
        private void PMSensorStageAlign()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMSensorStageAlign SensorStageAlign = (Run_PMSensorStageAlign)ModuleCamellia.CloneModuleRun("PMSensorStageAlign");
                ModuleCamellia.StartRun(SensorStageAlign);
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
                Run_PMSensorCameraTilt SensorCameraTilt = (Run_PMSensorCameraTilt)ModuleCamellia.CloneModuleRun("PMSensorCameraTilt");
                ModuleCamellia.StartRun(SensorCameraTilt);
            });
            thread.Start();

           
        }
    }
}
