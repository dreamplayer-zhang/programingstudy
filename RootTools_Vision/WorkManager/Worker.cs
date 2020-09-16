using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RootTools_Vision
{
    public enum WORKER_STATE
    {
        NONE = 0,
        WORK_ASSIGNED = 1,
        WORKING = 2,
        WORK_COMPLETED = 3
    }

    public delegate void EventWorkCompleted(object obj);

    public class Worker
    {
        #region [Task 관련 변수]
        private ManualResetEvent _waitSignal = new ManualResetEvent(false);

        bool isPause = false;

        Task task = null;

        CancellationTokenSource cancellationTokenSource;
        #endregion

        #region [Variables]
        private WORKER_STATE workerState;

        public WORKER_STATE eWorkerState
        {
            get { return this.workerState; }
            set
            {
                this.workerState = value;
            }
        }

        public event EventWorkCompleted WorkCompleted;

        WorkBundle works;

        #endregion

        public Worker()
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            this.task = Task.Factory.StartNew(() => { Run(); }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current); // 짧은 작업이 아닌 경우 LongRunning 옵션을 반드시 사용해야함. 자세한 것은 검색
        }

        ~Worker()
        {
            this.isPause = false;
            cancellationTokenSource.Cancel();
            _waitSignal.Set();
        }

        private void Run()
        {
            try
            {


                while (true)
                {
                    if (this.workerState != WORKER_STATE.WORK_ASSIGNED)
                    {
                        _waitSignal.WaitOne();
                    }

                    if (this.isPause == true)
                    {
                        _waitSignal.Reset();
                        _waitSignal.WaitOne();
                    }

                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    foreach (IWork work in this.works)
                    {
                        work.DoWork();
                    }


                    _waitSignal.Reset();

                    WorkDone();

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool SetWorkBundle(WorkBundle works)
        {
            this.works = works;
            this.workerState = WORKER_STATE.WORK_ASSIGNED;

            return true;
        }

        private void WorkDone()
        {
            this.workerState = WORKER_STATE.WORK_COMPLETED;

            if (WorkCompleted != null)
                this.WorkCompleted(this.works.Workplace);
        }

        public void Start()
        {
            this.isPause = false;
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

    }
}
