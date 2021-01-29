using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class Snap : WorkBase
    {
        public override WORK_TYPE Type =>  WORK_TYPE.SNAP;

        protected override bool Preparation()
        {
            return true;
        }

        protected override bool Execution()
        {
            return false;
        }
    }
}
