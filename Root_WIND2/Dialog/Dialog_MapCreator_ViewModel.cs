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

            this.imageViewerVM.SelectChipPointDone += SelectChipPointDone_Callback;
            this.imageViewerVM.SelectRoiPointDone += SelectRoiPointDone_Callback;
            this.imageViewerVM.SelectChipBoxDone += SelectChipBoxDone_Callback;
            this.imageViewerVM.SelectChipBoxReset += SelectChipBoxReset_Callback;
            this.imageViewerVM.SelectRoiBoxDone += SelectRoiBoxDone_Callback;
            this.imageViewerVM.SelectRoiBoxReset += SelectRoiBoxReset_Callback;
        }

        public void SelectChipPointDone_Callback()
        {
            this.ChipX = this.imageViewerVM.selectChipBox.Left;
            this.ChipY = this.imageViewerVM.selectChipBox.Bottom;
        }

        public void SelectRoiPointDone_Callback()
        {
            this.RoiX = this.imageViewerVM.selectRoiBox.Left;
            this.RoiY = this.imageViewerVM.selectRoiBox.Bottom;
        }

        public void SelectChipBoxDone_Callback()
        {
            this.ChipWidth = this.imageViewerVM.selectChipBox.Width;
            this.ChipHeight = this.imageViewerVM.selectChipBox.Height;
        }

        public void SelectChipBoxReset_Callback()
        {

        }

        public void SelectRoiBoxDone_Callback()
        {
            this.RoiWidth = this.imageViewerVM.selectRoiBox.Width;
            this.RoiHeight = this.imageViewerVM.selectRoiBox.Height;
        }

        public void SelectRoiBoxReset_Callback()
        {

        }

        #region [Properties]
        private int chipX = 0;
        public int ChipX
        {
            get => this.chipX;
            set
            {
                SetProperty<int>(ref this.chipX, value);
            }
        }

        private int chipY = 0;
        public int ChipY
        {
            get => this.chipY;
            set
            {
                SetProperty<int>(ref this.chipY, value);
            }
        }

        private int chipWidth = 0;
        public int ChipWidth
        {
            get => this.chipWidth;
            set
            {
                SetProperty<int>(ref this.chipWidth, value);
            }
        }

        private int chipHeight = 0;
        public int ChipHeight
        {
            get => this.chipHeight;
            set
            {
                SetProperty<int>(ref this.chipHeight, value);
            }
        }

        private int roiX = 0;
        public int RoiX
        {
            get => this.roiX;
            set
            {
                SetProperty<int>(ref this.roiX, value);
            }
        }

        private int roiY = 0;
        public int RoiY
        {
            get => this.roiY;
            set
            {
                SetProperty<int>(ref this.roiY, value);
            }
        }

        private int roiWidth = 0;
        public int RoiWidth
        {
            get => this.roiWidth;
            set
            {
                SetProperty<int>(ref this.roiWidth, value);
            }
        }

        private int roiHeight = 0;
        public int RoiHeight
        {
            get => this.roiHeight;
            set
            {
                SetProperty<int>(ref this.roiHeight, value);
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
