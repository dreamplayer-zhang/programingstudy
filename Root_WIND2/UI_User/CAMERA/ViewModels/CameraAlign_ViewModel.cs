using Root_WIND2.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class CameraAlign_ViewModel : ObservableObject
    {
        #region [Properties]
        private CameraAlign_ImageViewer_ViewModel imageViewerVM;
        public CameraAlign_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set
            {
                SetProperty<CameraAlign_ImageViewer_ViewModel>(ref this.imageViewerVM, value);
            }
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

        private Vision visionModule;
        public Vision VisionModule
        {
            get => this.visionModule;
        }

        private ObservableCollection<Image> featureItemList = new ObservableCollection<Image>();
        public ObservableCollection<Image> FeatureItemList
        {
            get => this.featureItemList;
            set
            {
                SetProperty<ObservableCollection<Image>>(ref this.featureItemList, value);
            }
        }

        private Image selectedFeatureItem = null;
        public Image SelectedFeatureItem
        {
            get => this.selectedFeatureItem;
            set
            {
                SetProperty<Image>(ref this.selectedFeatureItem, value);
            }
        }
        #endregion


        public CameraAlign_ViewModel()
        {
            this.imageViewerVM = new CameraAlign_ImageViewer_ViewModel();


            if (GlobalObjects.Instance.Get<WIND2_Engineer>().m_eMode == WIND2_Engineer.eMode.Vision)
            {
                this.visionModule = GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_Vision;

                motionControllerVM = new MotionController_ViewModel(VisionModule.AxisXY.p_axisX, VisionModule.AxisXY.p_axisY, VisionModule.AxisRotate, VisionModule.AxisZ);

                this.ImageViewerVM.SetImageData(visionModule.p_CamAlign.p_ImageViewer.p_ImageData);

                this.visionModule.p_CamAlign.Grabed += this.ImageViewerVM.OnUpdateImage;
            }

            //EQ.p_bStop = false;
            //Vision vision = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_Vision;
            //if (vision.p_eState != ModuleBase.eState.Ready)
            //{
            //    MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
            //    return;
            //}


            // 이거머냐
            //Run_GrabLineScan Grab = (Run_GrabLineScan)visionModule.CloneModuleRun("GrabLineScan");
            //var viewModel = new Dialog_Scan_ViewModel(visionModule, Grab);
            //Nullable<bool> result = GlobalObjects.Instance.Get<DialogService>().ShowDialog(viewModel);
            //if (result.HasValue)
            //{
            //    if (result.Value)
            //    {
            //        visionModule.StartRun(Grab);
            //    }
            //    else
            //    {

            //    }
            //}
        }

        #region [Properties]
        Axis m_axisX;
        public Axis p_axisX
        {
            get
            {
                return m_axisX;
            }
            set
            {
                SetProperty(ref m_axisX, value);
            }
        }

        Axis m_axisY;
        public Axis p_axisY
        {
            get
            {
                return m_axisY;
            }
            set
            {
                SetProperty(ref m_axisY, value);
            }
        }

        Axis m_axisZ;
        public Axis p_axisZ
        {
            get
            {
                return m_axisZ;
            }
            set
            {
                SetProperty(ref m_axisZ, value);
            }
        }

        Axis m_axisRotate;
        public Axis p_axisRotate
        {
            get
            {
                return m_axisRotate;
            }
            set
            {
                SetProperty(ref m_axisRotate, value);
            }
        }

        bool m_IsSlowCheck = true;
        public bool p_IsSlowCheck
        {
            get
            {
                return m_IsSlowCheck;
            }
            set
            {
                SetProperty(ref m_IsSlowCheck, value);
            }
        }
        #endregion

        #region [Command]
        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (VisionModule == null) return;

                    if (!VisionModule.p_CamAlign.m_ConnectDone)
                    {
                        VisionModule.p_CamAlign.FunctionConnect();
                    }
                    else
                    {
                        if(VisionModule.p_CamAlign.p_CamInfo._IsGrabbing == false)
                        {
                            VisionModule.p_CamAlign.GrabContinuousShot();
                        }
                    }

                    RefreshFeatureItemList();
                });
            }
        }

        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
                //VisionModule.p_CamAlign.StopGrab();
            });
        }

        public ICommand btnSetFirstAxisPositionCommond
        {
            get => new RelayCommand(() =>
            {


            });
        }

        public ICommand btnSetSecondAxisPositionCommond
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public ICommand btnFeatureAddCommand
        {
            get => new RelayCommand(() =>
            {
                string imagePath = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupCamera>().FeatureImagePath;
                DirectoryInfo di = new DirectoryInfo(imagePath);
                di.Create();

                string fileName = "feature_" + di.GetFiles().Length + ".bmp";

                this.ImageViewerVM.BoxImage.SaveImageSync(imagePath + "\\" + fileName);

                RefreshFeatureItemList();
            });
        }


       
        #endregion

        #region [Method]
        private void RefreshFeatureItemList()
        {
            this.FeatureItemList.Clear();

            string imagePath = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupCamera>().FeatureImagePath;

            DirectoryInfo di = new DirectoryInfo(imagePath);
            di.Create();

            foreach (FileInfo file in di.GetFiles()) 
            {
                Image image = new Image();
                string fullname = file.FullName;
                image.Source = Tools.ConvertBitmapToSource(((System.Drawing.Bitmap)System.Drawing.Image.FromFile(fullname)));
                image.Width = 200;
                image.Height = 200;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.FeatureItemList.Add(image);
                });
            }
        }

        #endregion
    }
}
