using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Root_WindII.Engineer;
using Root_EFEM;
using Root_EFEM.Module;

namespace Root_WindII
{
    public class RACAlignKey_ViewModel : ObservableObject, IPage
    {
        #region [Properties]
        private RACAlignKey_ImageViewer_ViewModel imageViewerVM;
        public RACAlignKey_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set
            {
                SetProperty<RACAlignKey_ImageViewer_ViewModel>(ref this.imageViewerVM, value);
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

        private MotionViewer_ViewModel motionViewerVM;
        public MotionViewer_ViewModel MotionViewerVM
        {
            get => this.motionViewerVM;
            set
            {
                SetProperty(ref this.motionViewerVM, value);
            }
        }

        private Vision_Frontside visionModule;
        public Vision_Frontside VisionModule
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


        public RACAlignKey_ViewModel()
        {
            this.imageViewerVM = new RACAlignKey_ImageViewer_ViewModel();

            this.visionModule = GlobalObjects.Instance.Get<WindII_Engineer>().m_handler.p_VisionFront;
            this.motionControllerVM = new MotionController_ViewModel(VisionModule.AxisXY.p_axisX, VisionModule.AxisXY.p_axisY, VisionModule.AxisRotate, VisionModule.AxisZ);
            this.motionViewerVM = new MotionViewer_ViewModel(VisionModule.AxisXY.p_axisX, VisionModule.AxisXY.p_axisY, VisionModule.AxisRotate, VisionModule.AxisZ);

            if (visionModule.p_CamVRS != null)
            {
                this.ImageViewerVM.SetImageData(visionModule.p_CamVRS.p_ImageViewer.p_ImageData);
                this.visionModule.p_CamVRS.Grabed += this.ImageViewerVM.OnUpdateImage;
            }
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
                    if(this.ImageViewerVM.p_ImageData != null)
                    {
                        this.ImageViewerVM.SetImageData(visionModule.p_CamVRS.p_ImageViewer.p_ImageData);
                        this.visionModule.p_CamVRS.Grabed += this.ImageViewerVM.OnUpdateImage;
                    }

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

                    LoadRecipe();
                });
            }
        }

        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
                VisionModule.p_CamVRS.StopGrab();
            });
        }

        public ICommand btnFeatureAddCommand
        {
            get => new RelayCommand(() =>
            {
                if (this.ImageViewerVM.BoxImage != null)
                {
                    FrontVRSAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontVRSAlignRecipe>();
                    ImageData featureImageData = this.ImageViewerVM.BoxImage;

                    byte[] srcBuf = featureImageData.m_aBuf;
                    byte[] rawData = new byte[featureImageData.p_Size.X * featureImageData.p_Size.Y * featureImageData.p_nByte];
                    Array.Copy(srcBuf, rawData, srcBuf.Length);

                    alignRecipe.AddAlignFeature(0, 0, featureImageData.p_Size.X, featureImageData.p_Size.Y, featureImageData.p_nByte, rawData);
                    alignRecipe.Save(Constants.RootPath.RootSetupRACAlignKeyPath);

                    RefreshFeatureItemList();
                }
            });
        }

        public ICommand btnFeatureDeleteCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedFeatureItem == null) return;

                FrontVRSAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontVRSAlignRecipe>();

                int index = FeatureItemList.IndexOf(SelectedFeatureItem);
                alignRecipe.AlignFeatureVRSList.RemoveAt(index);

                this.FeatureItemList.Remove(SelectedFeatureItem);
                SelectedFeatureItem = null;
            });
        }

        public ICommand btnFeatureClearCommand
        {
            get => new RelayCommand(() =>
            {
                ClearFeatureList();
            });
        }
        #endregion

        #region [Method]
        private void ClearFeatureList()
        {
            this.FeatureItemList.Clear();

            FrontVRSAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontVRSAlignRecipe>();
            alignRecipe.AlignFeatureVRSList.Clear();
        }

        private void RefreshFeatureItemList()
        {
            this.FeatureItemList.Clear();

            FrontVRSAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontVRSAlignRecipe>();

            foreach (RecipeType_ImageData feature in alignRecipe.AlignFeatureVRSList)
            {
                Image image = new Image();

                System.Drawing.Bitmap bmp = Tools.CovertArrayToBitmap(feature.RawData, feature.Width, feature.Height, feature.ByteCnt);
                image.Source = Tools.ConvertBitmapToSource(bmp);
                image.Width = 200;
                image.Height = 200;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.FeatureItemList.Add(image);
                });
            }
        }

        public void LoadRecipe()
        {
            FrontVRSAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontVRSAlignRecipe>();

            RefreshFeatureItemList();
        }
        #endregion
    }
}
