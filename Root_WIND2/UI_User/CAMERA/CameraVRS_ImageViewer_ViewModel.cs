using Root_WIND2.Module;
using RootTools;
using RootTools.Control;
using RootTools_Vision;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
    public class CameraVRS_ImageViewer_ViewModel : ObservableObject
    {
        Vision m_Vision;
        public Vision p_Vision
        {
            get
            {
                return m_Vision;
            }
            set
            {
                SetProperty(ref m_Vision, value);
            }
        }

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

        RootViewer_ViewModel m_RootViewer = new RootViewer_ViewModel();
        public RootViewer_ViewModel p_RootViewer
        {
            get
            {
                return m_RootViewer;
            }
            set
            {
                SetProperty(ref m_RootViewer, value);
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

        Dispatcher dispatcher = null;
        public CameraVRS_ImageViewer_ViewModel()
        {
            
            p_Vision = GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_Vision;

            p_axisX = p_Vision.AxisXY.p_axisX;
            p_axisY = p_Vision.AxisXY.p_axisY;
            p_axisZ = p_Vision.AxisZ;
            p_axisRotate = p_Vision.AxisRotate;

            p_Vision.p_CamAutoFocus.Grabed += OnUpdateImage;
            p_RootViewer.p_VisibleMenu = System.Windows.Visibility.Collapsed;
            p_RootViewer.p_ImageData = p_Vision.p_CamAutoFocus.p_ImageViewer.p_ImageData;
            dispatcher = Application.Current.Dispatcher;
        }

        private void OnUpdateImage(Object sender, EventArgs args)
        {
            dispatcher.Invoke(() =>
            {
                p_RootViewer.SetImageSource();
            });
        }

        #region [Command]
        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!p_Vision.p_CamAutoFocus.m_ConnectDone)
                    {
                        p_Vision.p_CamAutoFocus.FunctionConnect();
                    }
                    p_Vision.p_CamAutoFocus.GrabContinuousShot();
                });
            }
        }

        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
                p_Vision.p_CamAutoFocus.StopGrab();
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
