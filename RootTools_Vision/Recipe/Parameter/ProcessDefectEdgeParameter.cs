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
    }
}
