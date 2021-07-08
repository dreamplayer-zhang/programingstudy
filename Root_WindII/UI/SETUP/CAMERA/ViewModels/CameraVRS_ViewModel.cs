using RootTools;
using RootTools.Control;
using RootTools.Database;
using RootTools_Vision;
using System;
using System.Data;
using System.Reflection;
using System.Windows;
using Root_EFEM;
using Root_WindII.Engineer;

namespace Root_WindII
{
    public class CameraVRS_ViewModel : ObservableObject
    {
        #region [Properties]
        private readonly CameraVRS_ImageViewer_ViewModel imageViewerVM;
        public CameraVRS_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        private MotionController_ViewModel motionControllerVM;
        public MotionController_ViewModel MotionControllerVM
        {
            get => this.motionControllerVM;
            set
            {
                SetProperty(ref this.motionControllerVM, value);
            }
        }

        private Vision_Frontside visionModule;
        public Vision_Frontside VisionModule
        {
            get => this.visionModule;
        }

        private Database_DataView_VM m_DataViewer_VM;
        public Database_DataView_VM p_DataViewer_VM
        {
            get { return this.m_DataViewer_VM; }
            set { SetProperty(ref m_DataViewer_VM, value); }
        }

        private string inspectionID;
        public string InspectionID
        {
            get => this.inspectionID;
            set
            {
                SetProperty(ref this.inspectionID, value);
            }
        }
        #endregion

        public CameraVRS_ViewModel()
        {
            imageViewerVM = new CameraVRS_ImageViewer_ViewModel();

            this.visionModule = GlobalObjects.Instance.Get<WindII_Engineer>().m_handler.p_VisionFront;
            motionControllerVM = new MotionController_ViewModel(VisionModule.AxisXY.p_axisX, VisionModule.AxisXY.p_axisY, VisionModule.AxisRotate, VisionModule.AxisZ);

            if (visionModule.p_CamVRS.IsConnected() == true)
            {
                this.ImageViewerVM.SetImageData(VisionModule.p_CamVRS.p_ImageViewer.p_ImageData);
                this.visionModule.p_CamVRS.Grabed += this.ImageViewerVM.OnUpdateImage;
            }

            m_DataViewer_VM = new Database_DataView_VM();
            this.p_DataViewer_VM.SelectedCellsChanged += SelectedCellsChanged_Callback;
            //imageViewerVM.init();
        }
        private void SelectedCellsChanged_Callback(object obj)
        {
            DataRowView row = (DataRowView)obj;

            Defect defect = Tools.DataRowToDefect(row.Row);
            
            CPoint imgPos = new CPoint((int)defect.m_fAbsX, (int)defect.m_fAbsY);

            CPoint vrsPoint = PositionConverter.ConvertImageToVRS(VisionModule, imgPos);

            if(vrsPoint == null)
            {
                MessageBox.Show("Position Convert Error");
                return;
            }

            AxisXY axisXY = VisionModule.AxisXY;
            if (axisXY == null) return;

            if (VisionModule.Run(axisXY.WaitReady()))
            {
                MessageBox.Show("Axis Wait Ready Error");
                return;
            }
                
            if (VisionModule.Run(axisXY.StartMove(new RPoint(vrsPoint.X, vrsPoint.Y))))
            {
                MessageBox.Show("Axis Move Error");
                return;
            }

            if (VisionModule.Run(axisXY.WaitReady()))
            {
                MessageBox.Show("Axis Wait Ready Error");
                return;
            }

        }


        #region [Command]
        public System.Windows.Input.ICommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                if (VisionModule == null) return;

                if (!VisionModule.p_CamVRS.m_ConnectDone)
                {
                    VisionModule.p_CamVRS.FunctionConnect();
                }
                else
                {
                    if (VisionModule.p_CamVRS.p_CamInfo._IsGrabbing == false)
                    {
                        VisionModule.p_CamVRS.GrabContinuousShot();
                    }
                }
            });
        }
        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public RelayCommand btnInspectionIDSearchCommand
        {
            get => new RelayCommand(() =>
            {
                m_DataViewer_VM.pDataTable = DatabaseManager.Instance.SelectTablewithInspectionID("defect", this.inspectionID);
            });
        }
         
        public RelayCommand btnLoadCurrentInspectionCommand
        {
            get => new RelayCommand(() =>
            {
                m_DataViewer_VM.pDataTable = DatabaseManager.Instance.SelectCurrentInspectionDefect();

                this.InspectionID = DatabaseManager.Instance.GetInspectionID();
            });
        }

        #endregion
    }
}
