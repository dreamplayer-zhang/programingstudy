using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class ProcessDefect_EUVPod:WorkBase
    {
        public ProcessDefect_EUVPod() { }
        public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS_ALL;

         protected override bool Preparation()
        {
            return true;
        }

        protected override bool Execution()
        {
            ProcessDefectEUVPodParameter param = recipe.GetItem<ProcessDefectEUVPodParameter>();
            if (!param.Use) return true;

            return DoProcessDefect();
        }

        bool DoProcessDefect()
        {
            return false;
        }

    }
}
