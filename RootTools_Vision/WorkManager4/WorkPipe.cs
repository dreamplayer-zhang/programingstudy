using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager4
{
    internal class WorkPipe : IWorkPipe
    {
        #region [Fields]
        private WorkPipe nextPipe = null;

        WORK_TYPE type = WORK_TYPE.NONE;
        private int threadNum = 1;
        bool copyBuffer = false;
        bool waitAll = false;

        List<WorkBase> workList;

        #endregion

        public WorkPipe(WORK_TYPE type, int threadNum, bool copyBuffer = false, bool waitAll = false)
        {
            this.type = type;
            this.threadNum = threadNum;
            this.copyBuffer = copyBuffer;
            this.waitAll = waitAll;
        }

        public void AddStep(WorkPipe pipe)
        {
            WorkPipe temp = this.nextPipe;
            while(temp != null)
            {
                temp = temp.nextPipe;
            }
            temp.nextPipe = pipe;
        }

        public void SetWorks(WorkBundle workbundle)
        {
            this.workList = (from work in workbundle
                             where work.Type == this.type
                             select work).ToList();

            WorkPipe temp = this.nextPipe;
            temp.SetWorks(workbundle);
        }


        public void Enqueue(Workplace workplace)
        {
           // 계속 루프를 돌면서 큐 개수를 확인하면 cpu 점유율이 높아 질 수 있으므로
           // Enqueue하면 excute를 실행하게 해야함
           // excute는 thread 개수를...
        }

        public void DoWork()
        {
            //

            //try
            //{

            //}
            //catch (ThreadAbortException)
            //{
            //    // Abort nicely.
            //    Stop();
            //    Thread.ResetAbort();
            //}
            // Dequeue
            Workplace workplace = new Workplace();

            


            this.nextPipe.Enqueue(workplace);
        }
    }
}
