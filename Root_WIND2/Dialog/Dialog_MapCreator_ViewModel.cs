using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2
{
    class Dialog_MapCreator_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        private readonly Dialog_MapCreator_ImageViewer_ViewModel imageViewerVM;
        public Dialog_MapCreator_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        public Dialog_MapCreator_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr() == IntPtr.Zero && GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").m_eMode !=  ImageData.eMode.OtherPCMem)
                return;

            this.imageViewerVM = new Dialog_MapCreator_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());
        }

        #region [Properties]
        private int chipOriginX = 0;
        public int ChipOriginX
        {
            get => this.chipOriginX;
            set
            {
                SetProperty<int>(ref this.chipOriginX, value);
            }
        }

        private int chipOriginY = 0;
        public int ChipOriginY
        {
            get => this.chipOriginY;
            set
            {
                SetProperty<int>(ref this.chipOriginY, value);
            }
        }

        private int chipOriginWidth = 0;
        public int ChipOriginWidth
        {
            get => this.chipOriginWidth;
            set
            {
                SetProperty<int>(ref this.chipOriginWidth, value);
            }
        }

        private int chipOriginHeight = 0;
        public int ChipOriginHeight
        {
            get => this.chipOriginHeight;
            set
            {
                SetProperty<int>(ref this.chipOriginHeight, value);
            }
        }

        private int roiOriginX = 0;
        public int RoiOriginX
        {
            get => this.roiOriginX;
            set
            {
                SetProperty<int>(ref this.roiOriginX, value);
            }
        }

        private int roiOriginY = 0;
        public int RoiOriginY
        {
            get => this.roiOriginY;
            set
            {
                SetProperty<int>(ref this.roiOriginY, value);
            }
        }

        private int roiOriginWidth = 0;
        public int RoiOriginWidth
        {
            get => this.roiOriginWidth;
            set
            {
                SetProperty<int>(ref this.roiOriginWidth, value);
            }
        }

        private int roiOriginHeight = 0;
        public int RoiOriginHeight
        {
            get => this.roiOriginHeight;
            set
            {
                SetProperty<int>(ref this.roiOriginHeight, value);
            }
        }
        #endregion

        #region [Command]
        public RelayCommand btnStopFindChipCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        public RelayCommand btnStartFindChipCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        public void OnOkButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }
        #endregion

    }
}
