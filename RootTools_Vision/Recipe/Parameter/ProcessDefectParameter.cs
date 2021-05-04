using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class ProcessDefectParameter : ParameterBase, IOptionParameter
    {
        public ProcessDefectParameter() : base(typeof(ProcessDefect))
        {
        }

        private bool useProcessDefect = true;
        public bool Use
        {
            get => this.useProcessDefect;
            set
            {
                SetProperty(ref useProcessDefect, value);
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
