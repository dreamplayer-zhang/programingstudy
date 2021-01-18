using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class ProcessDefect : WorkBase
    {
        public ProcessDefect()
        {
        }

        public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS;


        protected override bool Preparation()
        {
            return true;
        }

        protected override bool Execution()
        {
            DoProcessDefect();
            return true;
        }

        public void DoProcessDefect()
        {


            WorkEventManager.OnProcessDefectDone(this.currentWorkplace, new ProcessDefectDoneEventArgs());
        }
    }
}
