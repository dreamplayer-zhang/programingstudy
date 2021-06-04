using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class ProcessDefectWaferParameter : ParameterBase, IOptionParameter
    {
        public ProcessDefectWaferParameter() : base(typeof(ProcessDefect_Wafer))
        {
        }

        private bool useProcessDefectWafer = true;
        public bool Use
        {
            get => this.useProcessDefectWafer;
            set
            {
                SetProperty(ref useProcessDefectWafer, value);
            }
        }

        private bool useMergeDefect = true;
        public bool UseMergeDefect
        {
            get => this.useMergeDefect;
            set
            {
                SetProperty(ref this.useMergeDefect, value);
            }
        }

        private int mergeDefectDistance = 2;
        public int MergeDefectDistnace
        {
            get => this.mergeDefectDistance;
            set
            {
                SetProperty(ref this.mergeDefectDistance, value);
            }
        }
    }
}
