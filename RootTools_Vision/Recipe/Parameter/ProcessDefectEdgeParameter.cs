using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class ProcessDefectEdgeParameter : ParameterBase
	{
        public ProcessDefectEdgeParameter() : base(typeof(ProcessDefect_Edge))
        {
        }

        private bool useProcessDefectEdge = true;
        public bool Use
        {
            get => this.useProcessDefectEdge;
            set
            {
                SetProperty(ref useProcessDefectEdge, value);
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
