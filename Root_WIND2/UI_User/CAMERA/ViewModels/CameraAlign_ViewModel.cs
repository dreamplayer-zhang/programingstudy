using Root_WIND2.Module;
using RootTools.Control;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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



            if (GlobalObjects.Instance.Get<WIND2_Engineer>().m_eMode == WIND2_Engineer.eMode.Vision)
            {
                this.visionModule = GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_Vision;

                p_axisX = VisionModule.AxisXY.p_axisX;
                p_axisY = VisionModule.AxisXY.p_axisY;
                p_axisZ = VisionModule.AxisZ;
                p_axisRotate = VisionModule.AxisRotate;

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
                    if (!VisionModule.p_CamAlign.m_ConnectDone)
                    {
                        VisionModule.p_CamAlign.FunctionConnect();
                    }
                    VisionModule.p_CamAlign.GrabContinuousShot();
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

        public ICommand CmdAxisXMoveLeft
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!p_IsSlowCheck)
                    {
                        p_axisX.Jog(-1);
                    }
                    else
                    {
                        p_axisX.Jog(-0.31);
                    }
                });
            }
        }

        public ICommand CmdAxisXMoveRight
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!p_IsSlowCheck)
                    {
                        p_axisX.Jog(1);
                    }
                    else
                    {
                        p_axisX.Jog(0.31);
                    }
                });
            }
        }

        public ICommand CmdAxisYMoveUp
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!p_IsSlowCheck)
                    {
                        p_axisY.Jog(1);
                    }
                    else
                    {
                        p_axisY.Jog(0.31);
                    }
                });
            }
        }

        public ICommand CmdAxisYMoveDown
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!p_IsSlowCheck)
                    {
                        p_axisY.Jog(-1);
                    }
                    else
                    {
                        p_axisY.Jog(-0.31);
                    }
                });
            }
        }

        public ICommand CmdAxisZMoveUp
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!p_IsSlowCheck)
                    {
                        p_axisZ.Jog(-1);
                    }
                    else
                    {
                        p_axisZ.Jog(-0.31);
                    }
                });
            }
        }

        public ICommand CmdAxisZMoveDown
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!p_IsSlowCheck)
                    {
                        p_axisZ.Jog(1);
                    }
                    else
                    {
                        p_axisZ.Jog(0.31);
                    }
                });
            }
        }

        public ICommand CmdXStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_axisX.StopAxis();
                });
            }
        }

        public ICommand CmdYStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_axisY.StopAxis();
                });
            }
        }

        public ICommand CmdZStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_axisZ.StopAxis();
                });
            }
        }


        #endregion
    }
}
