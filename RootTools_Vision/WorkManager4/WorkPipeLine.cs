using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager4
{
    internal class WorkPipeLine : IWorkPipeLine
    {
        #region [Field]
        WorkPipe workPipe;

        private int threadNum = 1;
        private bool copyBuffer = false;
        #endregion

        public WorkPipeLine(int inspectionThreadNum = 1, bool copyBuffer = false)
        {
            this.threadNum = inspectionThreadNum;
            this.copyBuffer = copyBuffer;

            CreatePipes();
        }

        private void CreatePipes()
        {
            this.workPipe = new WorkPipe(WORK_TYPE.SNAP, 1, false, false);
            this.workPipe.AddStep(new WorkPipe(WORK_TYPE.ALIGNMENT, 1, false, false));
            this.workPipe.AddStep(new WorkPipe(WORK_TYPE.INSPECTION, threadNum, true, false));
            this.workPipe.AddStep(new WorkPipe(WORK_TYPE.DEFECTPROCESS, threadNum, false, false));
            this.workPipe.AddStep(new WorkPipe(WORK_TYPE.DEFECTPROCESS_ALL, 1, false, true));
        }

        public void SetWork(WorkBundle works)
        {
            
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
