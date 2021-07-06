using RootTools.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class MotionViewer_ViewModel : ObservableObject
    {

        Axis axisX;
        Axis axisY;
        Axis axisTheta;
        Axis axisZ;


        public MotionViewer_ViewModel(Axis axisX, Axis axisY, Axis axisTheta, Axis axisZ)
        {
            this.axisX = axisX;
            this.axisY = axisY;
            this.axisTheta = axisTheta;
            this.axisZ = axisZ;
        }

        #region [Properties]
        public Axis AxisPositionX
        {
            get => this.axisX;
            set
            {
                SetProperty(ref this.axisX, value);
            }
        }

        public Axis AxisPositionY
        {
            get => this.axisY;
            set
            {
                SetProperty(ref this.axisY, value);
            }
        }

        public Axis AxisPositionTheta
        {
            get => this.axisTheta;
            set
            {
                SetProperty(ref this.axisTheta, value);
            }
        }

        public Axis AxisPositionZ
        {
            get => this.axisZ;
            set
            {
                SetProperty(ref this.axisZ, value);
            }
        }

        #endregion
    }
}
