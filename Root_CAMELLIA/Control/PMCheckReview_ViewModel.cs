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
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    public class PMCheckReview_ViewModel : ObservableObject
    {
        private RootViewer_ViewModel m_rootViewer = new RootViewer_ViewModel();
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

        private BitmapSource m_pmImgSource;
        public BitmapSource p_pmImgSource
        {
            get
            {
                return m_pmImgSource;
            }
            set
            {
                SetProperty(ref m_pmImgSource, value);
            }
        }

        public PMCheckReview_ViewModel()
        {
            Init();
        }

        public void Init()
        {
            //pointListItem = new DataTable();
            p_DataTable.Columns.Add(new DataColumn("TIme"));
            p_DataTable.Columns.Add(new DataColumn("List"));
            p_DataTable.Columns.Add(new DataColumn("Result"));

            dispatcher = Application.Current.Dispatcher;

            ModuleCamellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;

            ModuleCamellia.p_CamVRS.Captured += GetImage;
        }

        private Dispatcher dispatcher;
        public void GetImage(object sender, EventArgs e)
        {
            //ImageData p_ImageData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;
            BitmapSource bitmapSource = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData.GetBitMapSource(3);
            p_rootViewer.p_ImageData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;


            //p_pmImgSource = bitmapSource;
            //p_rootViewer.p_ImageData = p_ImageData;
            //dispatcher.Invoke(() =>
            //{
            //    p_rootViewer.SetImageData(p_ImageData);
            //});
            //RootTools.Camera.BaslerPylon.Camera_Basler p_CamVRS = ModuleCamellia.p_CamVRS;
            //Mat mat = new Mat(new System.Drawing.Size(p_CamVRS.GetRoiSize().X, p_CamVRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, p_CamVRS.p_ImageViewer.p_ImageData.GetPtr(), (int)p_CamVRS.p_ImageViewer.p_ImageData.p_Stride * 3);
            ////CvInvoke.Imshow("test",mat);
            ////CvInvoke.WaitKey(0);
            ////CvInvoke.DestroyAllWindows();
            //BitmapSource bit = CreateBitmapSourceFromBitmap(mat.Bitmap);
            //p_pmImgSource = bit;

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

        public ICommand CmdSensorStageAlign
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
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMReflectance ReflectanceRepeatability = (Run_PMReflectance)ModuleCamellia.CloneModuleRun("PMReflectance");
                ModuleCamellia.StartRun(ReflectanceRepeatability) ;
                //ReflectanceRepeatability.Run();
            });
            thread.Start();
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
                Run_PMSensorCameraTilt SensorCameraTilt= (Run_PMSensorCameraTilt)ModuleCamellia.CloneModuleRun("PMSensorCameraTilt");
                ModuleCamellia.StartRun(SensorCameraTilt);
            });
            thread.Start();
        }
    }
}
