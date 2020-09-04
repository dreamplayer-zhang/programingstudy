using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using RootTools;

namespace RootTools_Vision
{
    #region [Event From Worker]
    public delegate void EventChangedWorkState(WORK_TYPE work_type, WORKER_STATE worker_state, WORKPLACE_STATE workplace_state, int indexWorkplace, Point subIndex);
    public delegate void EventReadyToWork(WORK_TYPE work_type, Workplace workplace);
    public delegate void EventWorkCompleted();
    #endregion

    /// <summary>
    /// 작업 수행
    /// </summary>
    public class Worker : IWorkable
    {
        #region [Event]        
        public event EventChangedWorkState ChangedWorkState;
        public event EventReadyToWork ReadyToWork;
        public event EventWorkCompleted WorkCompleted;


        //
        private ManualResetEvent _waitSignal = new ManualResetEvent(false);
        #endregion

        //
        #region [Member Variables]
        bool isPause = false;


        #endregion


        //
        private WORKER_STATE workerState;

        public WORKER_STATE eWorkerState
        {
            get { return this.workerState; }
            set 
            {
                this.workerState = value;
            }
        }


        Task task = null;

        CancellationTokenSource cancellationTokenSource;

        Workplace workplace;
        WorkBundle works;


        public Worker()
        {
            //task = Task.Run(Run, token);
            // 스레드에 작업량에 따라서 LongRunning을 사용할 지, 일반 Task.Run을 사용할지 결쟁 ※테스트 필요

            workerState = WORKER_STATE.NONE;

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            this.task = Task.Factory.StartNew(() => { DoWork(); }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current); // 짧은 작업이 아닌 경우 LongRunning 옵션을 반드시 사용해야함. 자세한 것은 검색

            this.WorkCompleted += WorkCompleted_Callback;
        }

        ~Worker()
        {
            this.isPause = false;
            cancellationTokenSource.Cancel();
            _waitSignal.Set();
        }

        public void InitWorker()
        {

        }

        public bool AssignWork(Workplace workplace, WorkBundle works)
        {
            this.workplace = workplace;
            this.works = works;
            this.workerState = WORKER_STATE.WORK_ASSIGNED;

            workplace.State = WORKPLACE_STATE.OCCUPIED;
            
            return true;
        }


        // Workplace와 Work를 수행한다.

        private void DoWork()
        {
            while (true)
            {
                if (this.workerState != WORKER_STATE.WORK_ASSIGNED)
                {
                    _waitSignal.WaitOne();
                }

                if(this.isPause == true)
                {
                    _waitSignal.Reset();
                    _waitSignal.WaitOne();
                }

                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                InspectionResultCollection InspResult = null;

                foreach (IWork work in this.works)
                {
                    //Defect 타입 선언

                    this.ChangedWorkState(work.TYPE, this.workerState, this.workplace.State, this.workplace.Index, this.workplace.SubIndex);
                    this.ReadyToWork(work.TYPE, this.workplace);
                    switch (work.TYPE)
                    {
                        case WORK_TYPE.Position:
                            TempWork();
                            //MessageBox.Show("Position");
                            break;
                        case WORK_TYPE.PreInspection:
                            TempWork();
                            //MessageBox.Show("PreInspection");
                            break;
                        case WORK_TYPE.Inspection:
                            //TempWork();
                            ((IInspection)work).DoInspection(this.workplace, out InspResult);
                            TempWork();
                            //MessageBox.Show(((IInspection)work).TYPE.ToString());
                            break;
                        case WORK_TYPE.Measurement:
                            TempWork();
                            //MessageBox.Show("Measurement");
                            break;

                        case WORK_TYPE.ProcessDefect:
                            // 20200825
                            break;
                    }
                }
                _waitSignal.Reset();
                this.WorkCompleted();
            }
        }

        private void TempWork()
        {
            int nCount = 10000;
            int sec = 10000;
            long sum = 0;
            for(int i = 0; i < sec; i++)
            {
                for (int j = 0; j < nCount; j++)
                {
                    sum += i;
                }
            }
        }

        public void Start()
        {
            this.workerState = WORKER_STATE.WORKING;
            _waitSignal.Set();
        }

        public void Pause()
        {
            this.isPause = true;
        }

        public void Stop()
        {
            this.isPause = true;
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }



        public bool IsRunning()
        {
            if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                return false;
            else
                return true;
        }

        private void WorkCompleted_Callback()
        {
            
            this.workerState = WORKER_STATE.WORK_COMPLETED;
            if (this.ChangedWorkState != null) 
                this.ChangedWorkState(WORK_TYPE.None, this.workerState, this.workplace.State, this.workplace.Index, this.workplace.SubIndex);
        }
    }
}
