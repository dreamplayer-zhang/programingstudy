using RootTools;
using RootTools.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RootTools_Vision
{
    public class MotionController_ViewModel : ObservableObject
    {

        const double AXIS_SLOW_SPEED = 0.3;
        const double AXIS_FAST_SPEED = 1.0;
        private double axisSpeed = AXIS_SLOW_SPEED;
        private double axisZSpeed = 1;

        public MotionController_ViewModel(Axis axisX, Axis axisY, Axis axisTh, Axis axisZ)
        {
            this.AxisX = axisX;
            this.AxisY = axisY;
            this.AxisZ = axisZ;
            this.AxisRotate = axisTh;
        }

        #region [Properties]
        Axis axisX;
        public Axis AxisX
        {
            get
            {
                return axisX;
            }
            set
            {
                SetProperty(ref axisX, value);
            }
        }

        Axis axisY;
        public Axis AxisY
        {
            get
            {
                return axisY;
            }
            set
            {
                SetProperty(ref axisY, value);
            }
        }

        Axis axisZ;
        public Axis AxisZ
        {
            get
            {
                return axisZ;
            }
            set
            {
                SetProperty(ref axisZ, value);
            }
        }

        Axis axisRotate;
        public Axis AxisRotate
        {
            get
            {
                return axisRotate;
            }
            set
            {
                SetProperty(ref axisRotate, value);
            }
        }

        bool isFast = false;
        public bool IsFast
        {
            get
            {
                return isFast;
            }
            set
            {
                if(value == true)
                {
                    axisSpeed = AXIS_FAST_SPEED;
                }
                else
                {
                    axisSpeed = AXIS_SLOW_SPEED;
                }
                SetProperty(ref isFast, value);
            }
        }

        bool isManual = false;
        public bool IsManual
        {
            get
            {
                return isManual;
            }
            set
            {
                SetProperty(ref isManual, value);
            }
        }

        private double manualSpeed;
        public double ManualSpeed
        {
            get => this.manualSpeed;
            set
            {
                SetProperty(ref this.manualSpeed, value);
            }
        }


        public double AxisZSpeed
        {
            get => this.axisZSpeed;
            set
            {
                SetProperty(ref this.axisZSpeed, value);
            }
        }
        #endregion

        #region [Command]
        public ICommand CmdAxisXMoveLeft
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisX?.Jog(IsManual ? ManualSpeed : - axisSpeed);
                });
            }
        }

        public ICommand CmdAxisXMoveRight
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisX?.Jog(IsManual ? ManualSpeed : axisSpeed);
                });
            }
        }

        public ICommand CmdAxisYMoveUp
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisY?.Jog(IsManual ? ManualSpeed : axisSpeed);
                });
            }
        }

        public ICommand CmdAxisYMoveDown
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisY?.Jog(IsManual ? ManualSpeed : -axisSpeed);
                });
            }
        }

        public ICommand CmdAxisThetaMoveDown
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisRotate?.Jog(IsManual ? ManualSpeed : -axisSpeed);
                });
            }
        }

        public ICommand CmdAxisThetaMoveUp
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisRotate?.Jog(IsManual ? ManualSpeed : axisSpeed);
                });
            }
        }

        public ICommand CmdAxisZMoveUp
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisZ?.Jog(IsManual ? ManualSpeed : -axisZSpeed);
                });
            }
        }

        public ICommand CmdAxisZMoveDown
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisZ?.Jog(axisZSpeed);
                });
            }
        }

        public ICommand CmdXStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisX?.StopAxis();
                });
            }
        }

        public ICommand CmdYStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisY?.StopAxis();
                });
            }
        }

        public ICommand CmdThetaStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisRotate?.StopAxis();
                });
            }
        }

        public ICommand CmdZStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    axisZ?.StopAxis();
                });
            }
        }
        #endregion
    }
}
