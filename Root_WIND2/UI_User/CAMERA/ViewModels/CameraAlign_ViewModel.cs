using Root_WIND2.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private Vision visionModule;
        public Vision VisionModule
        {
            get => this.visionModule;
        }
        #endregion

        public CameraAlign_ViewModel()
        {
            this.imageViewerVM = new CameraAlign_ImageViewer_ViewModel();

            this.visionModule = GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_Vision;
            //p_RootViewer.p_ImageData = p_Vision..p_ImageViewer.p_ImageData;

        }

        #region [Command]
        public RelayCommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {


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
