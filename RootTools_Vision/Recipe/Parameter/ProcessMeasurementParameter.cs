using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class ProcessMeasurementParameter : ParameterBase
    {
        public ProcessMeasurementParameter() : base(typeof(ProcessMeasurement))
        {
        }

        private bool useProcessMeasurement = true;
        public bool Use
        {
            get => this.useProcessMeasurement;
            set
            {
                SetProperty(ref useProcessMeasurement, value);
            }
        }
    }
}
