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
    }
}
