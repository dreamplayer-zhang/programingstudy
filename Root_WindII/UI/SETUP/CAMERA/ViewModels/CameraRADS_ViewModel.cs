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
    public class CameraRADS_ViewModel : ObservableObject
    {
        #region [Properties]
        private readonly CameraRADS_ImageViewer_ViewModel imageViewerVM;
        public CameraRADS_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        private Vision_Frontside visionModule;
        public Vision_Frontside VisionModule
        {
            get => this.visionModule;
        }
        #endregion

        public CameraRADS_ViewModel()
        {
            imageViewerVM = new CameraRADS_ImageViewer_ViewModel();

            this.visionModule = GlobalObjects.Instance.Get<WindII_Engineer>().m_handler.p_VisionFront;

            if (visionModule.p_CamRADS != null &&visionModule.p_CamRADS.IsConnected() == true)
            {
                this.ImageViewerVM.SetImageData(VisionModule.p_CamRADS.p_ImageViewer.p_ImageData);
                this.visionModule.p_CamRADS.Grabed += this.ImageViewerVM.OnUpdateImage;
            }

            //imageViewerVM.init();
        }

        #region [Command]
        public System.Windows.Input.ICommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                if (VisionModule == null) return;

                if (!VisionModule.p_CamRADS.m_ConnectDone)
                {
                    VisionModule.p_CamRADS.FunctionConnect();
                }
                else
                {
                    if (VisionModule.p_CamRADS.p_CamInfo._IsGrabbing == false)
                    {
                        VisionModule.p_CamRADS.GrabContinuousShot();
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
        #endregion
    }
}
