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
using Root_EFEM.Module.FrontsideVision;
using RootTools.Light;

namespace Root_WindII
{
    public class CameraAlign_ViewModel : ObservableObject, IPage
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

        private long firstAxisPositionX = 0;
        public long FirstAxisPositionX
        {
            get => this.firstAxisPositionX;
            set
            {
                SetProperty(ref this.firstAxisPositionX, value);
            }
        }

        private long firstAxisPositionY = 0;
        public long FirstAxisPositionY
        {
            get => this.firstAxisPositionY;
            set
            {
                SetProperty(ref this.firstAxisPositionY, value);
            }
        }

        private long secondAxisPositionX = 0;
        public long SecondAxisPositionX
        {
            get => this.secondAxisPositionX;
            set
            {
                SetProperty(ref this.secondAxisPositionX, value);
            }
        }

        private long secondAxisPositionY = 0;
        public long SecondAxisPositionY
        {
            get => this.secondAxisPositionY;
            set
            {
                SetProperty(ref this.secondAxisPositionY, value);
            }
        }
        #endregion

        public CameraAlign_ViewModel()
        {
            this.imageViewerVM = new CameraAlign_ImageViewer_ViewModel();

            this.visionModule = GlobalObjects.Instance.Get<WindII_Engineer>().m_handler.p_VisionFront;
            this.motionControllerVM = new MotionController_ViewModel(VisionModule.AxisXY.p_axisX, VisionModule.AxisXY.p_axisY, VisionModule.AxisRotate, VisionModule.AxisZ);
            this.motionViewerVM = new MotionViewer_ViewModel(VisionModule.AxisXY.p_axisX, VisionModule.AxisXY.p_axisY, VisionModule.AxisRotate, VisionModule.AxisZ);

            if(visionModule.p_CamAlign != null)
            {
                this.ImageViewerVM.SetImageData(visionModule.p_CamAlign.p_ImageViewer.p_ImageData);
                this.visionModule.p_CamAlign.Grabed += this.ImageViewerVM.OnUpdateImage;
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
                        this.ImageViewerVM.SetImageData(visionModule.p_CamAlign.p_ImageViewer.p_ImageData);
                        this.visionModule.p_CamAlign.Grabed += this.ImageViewerVM.OnUpdateImage;
                    }

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

                    #region [조명 설정]
                    SettingItem_SetupCamera setupCamera = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupCamera>();

                    string illumIndexListString = setupCamera.IlluminationIndexList.Trim();
                    List<int> illumIndexList = new List<int>();
                    if (illumIndexListString != "")
                    {


                        string[] stringArray = illumIndexListString.Split(',');
                        foreach (string indexStr in stringArray)
                        {
                            illumIndexList.Add(Convert.ToInt32(indexStr));
                        }
                    }

                    Vision_Frontside vision = ((WindII_Handler)GlobalObjects.Instance.Get<WindII_Engineer>().ClassHandler()).p_VisionFront;

                    Run_VisionAlign alignModule = (Run_VisionAlign)vision.CloneModuleRun("VisionAlign");
                    GrabModeFront grabMode = alignModule.m_grabMode;
                    if(grabMode != null)
                    {
                        //for(int i= 0; i < grabMode.m_lightSet.m_aLight.Count; i++)
                        //{
                        //    if()
                        //    grabMode.m_lightSet.m_aLight[i]
                        //}

                        List<Light> lightList = new List<Light>();
                        foreach (var index in illumIndexList)
                        {
                            if (index < grabMode.m_lightSet.m_aLight.Count)
                            {
                                lightList.Add(grabMode.m_lightSet.m_aLight[index]);
                            }
                        }


                        // for (int n = 0; n < m_aLightPower.Count; n++)
                        //{
                        //    if (m_lightSet.m_aLight[n].m_light != null)
                        //        m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
                        //}
                    }
                    #endregion

                    LoadRecipe();
                });
            }
        }

        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
                VisionModule.p_CamAlign.StopGrab();
            });
        }

        public ICommand btnSetFirstAxisPositionCommond
        {
            get => new RelayCommand(() =>
            {
                this.FirstAxisPositionX = (long)VisionModule.AxisXY.p_axisX.p_posActual;
                this.FirstAxisPositionY = (long)VisionModule.AxisXY.p_axisY.p_posActual;

                FrontAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontAlignRecipe>();

                alignRecipe.FirstSearchPointX = this.FirstAxisPositionX;
                alignRecipe.FirstSearchPointY = this.FirstAxisPositionY;

            });
        }

        public ICommand btnSetSecondAxisPositionCommond
        {
            get => new RelayCommand(() =>
            {
                this.SecondAxisPositionX = (long)VisionModule.AxisXY.p_axisX.p_posActual;
                this.SecondAxisPositionY = (long)VisionModule.AxisXY.p_axisY.p_posActual;

                FrontAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontAlignRecipe>();

                alignRecipe.SecondSearchPointX = this.SecondAxisPositionX;
                alignRecipe.SecondSearchPointY = this.SecondAxisPositionY;
            });
        }

        public ICommand btnFeatureAddCommand
        {
            get => new RelayCommand(() =>
            {
                FrontAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontAlignRecipe>();

                ImageData featureImageData = this.ImageViewerVM.BoxImage;

                byte[] srcBuf = featureImageData.m_aBuf;
                byte[] rawData = new byte[featureImageData.p_Size.X * featureImageData.p_Size.Y * featureImageData.p_nByte];
                Array.Copy(srcBuf, rawData, srcBuf.Length);

                alignRecipe.AddAlignFeature(0, 0, featureImageData.p_Size.X, featureImageData.p_Size.Y, featureImageData.p_nByte, rawData);

                RefreshFeatureItemList();
            });
        }

        public ICommand btnFeatureDeleteCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedFeatureItem == null) return;

                FrontAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontAlignRecipe>();

                int index = FeatureItemList.IndexOf(SelectedFeatureItem);
                alignRecipe.AlignFeatureList.RemoveAt(index);

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
            this.featureItemList.Clear();

            FrontAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontAlignRecipe>();
            alignRecipe.AlignFeatureList.Clear();
        }

        private void RefreshFeatureItemList()
        {
            this.FeatureItemList.Clear();

            FrontAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontAlignRecipe>();

            foreach (RecipeType_ImageData feature in alignRecipe.AlignFeatureList) 
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
            FrontAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontAlignRecipe>();

            this.FirstAxisPositionX = alignRecipe.FirstSearchPointX;
            this.FirstAxisPositionY = alignRecipe.FirstSearchPointY;
            this.SecondAxisPositionX = alignRecipe.SecondSearchPointX;
            this.SecondAxisPositionY = alignRecipe.SecondSearchPointY;

            RefreshFeatureItemList();
        }
        #endregion
    }
}
