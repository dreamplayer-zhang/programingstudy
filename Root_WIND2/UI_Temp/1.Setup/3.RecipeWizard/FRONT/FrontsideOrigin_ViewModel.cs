using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WIND2.UI_Temp
{
    class FrontsideOrigin_ViewModel : ObservableObject
    {
        private readonly FrontsideOrigin_ImageViewer_ViewModel imageViewerVM;
        public FrontsideOrigin_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }


        public FrontsideOrigin_ViewModel()
        {
            this.imageViewerVM = new FrontsideOrigin_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            this.imageViewerVM.ViewerStateChanged += ViewerStateChanged_Callback;
            this.imageViewerVM.OriginBoxReset += OriginBoxReset_Callback;
            this.imageViewerVM.OriginBoxDone += OriginBoxDone_Callback;
        }

        public void  ViewerStateChanged_Callback()
        {
            this.DisplayViewerState = this.imageViewerVM.ViewerState.ToString();
            if(this.ImageViewerVM.ViewerState == FRONT_ORIGIN_VIEWER_STATE.Normal)
            {
                this.IsOriginChecked = false;
                this.IsPitchChecked = false;
                this.IsRularChecked = false;
            }
        }

        public void OriginBoxReset_Callback()
        {
            this.IsPitchEnable = false;
        }

        public void OriginBoxDone_Callback()
        {
            this.IsPitchEnable = true;
        }

        #region [Properties]
        private string displayViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal.ToString();
        public string DisplayViewerState
        {
            get => this.displayViewerState;
            set
            {
                SetProperty<string>(ref this.displayViewerState, value);
            }
        }

        private bool isOriginChecked = false;
        public bool IsOriginChecked
        {
            get => this.isOriginChecked;
            set
            {
                if (value == true)
                {
                    this.IsPitchChecked = false;
                    this.IsRularChecked = false;
                }
                SetProperty<bool>(ref this.isOriginChecked, value);
            }
        }

        private bool isPitchChecked = false;
        public bool IsPitchChecked
        {
            get => this.isPitchChecked;
            set
            {
                if (value == true)
                {
                    this.IsOriginChecked = false;
                    this.IsRularChecked = false;
                }
                SetProperty<bool>(ref this.isPitchChecked, value);
            }
        }


        private bool isPitchEnable = false;
        public bool IsPitchEnable
        {
            get => this.isPitchEnable;
            set
            {
                SetProperty<bool>(ref this.isPitchEnable, value);
            }
        }

        private bool isRularChecked = false;
        public bool IsRularChecked
        {
            get => this.isRularChecked;
            set
            {
                if (value == true)
                {
                    this.IsPitchChecked = false;
                    this.IsOriginChecked = false;
                }
                SetProperty<bool>(ref this.isRularChecked, value);
            }
        }

        #endregion

        #region [Command]
        public RelayCommand btnOriginCommand
        {
            get 
            {
                return new RelayCommand(() =>
                {
                    if(this.IsOriginChecked == true)
                    {
                        this.ImageViewerVM.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Origin;
                        this.DisplayViewerState = this.ImageViewerVM.ViewerState.ToString();
                    }
                    else
                    {
                        this.ImageViewerVM.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ImageViewerVM.ViewerState.ToString();

                    }
                });
            }
        }

        public RelayCommand btnPitchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsPitchChecked == true)
                    {
                        this.ImageViewerVM.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Pitch;
                        this.DisplayViewerState = this.ImageViewerVM.ViewerState.ToString();
                    }
                    else
                    {
                        this.ImageViewerVM.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ImageViewerVM.ViewerState.ToString(); 
                    }
                });
            }
        }

        public RelayCommand btnRularCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsRularChecked == true)
                    {
                        this.ImageViewerVM.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Rular;
                        this.DisplayViewerState = this.ImageViewerVM.ViewerState.ToString();
                    }
                    else
                    {
                        this.ImageViewerVM.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ImageViewerVM.ViewerState.ToString();
                    }
                });
            }
        }

        public RelayCommand btnOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ImageViewerVM._openImage();
                });
            }
        }

        public RelayCommand btnSaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ImageViewerVM._saveImage();
                });
            }
        }

        public RelayCommand btnClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ImageViewerVM._clearImage();
                });
            }
        }

        public RelayCommand btnViewFullCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ImageViewerVM.DisplayFull();
                });
            }
        }

        public RelayCommand btnViewBoxCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ImageViewerVM.DisplayBox();
                });
            }
        }


        #endregion






    }
}
