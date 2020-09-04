using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public enum WORKER_STATE
    {
        NONE = 0,
        WORK_ASSIGNED = 1,
        WORKING = 2,
        WORK_COMPLETED = 3
    }

    /// <summary>
    /// Worker 생성/관리
    /// WokerManager에서 작업할당은 안함
    /// </summary>
    public class WorkerManager : IWorkable
    {
        #region [Event]
        public event EventChangedWorkState ChangedWorkState;
        public event EventReadyToWork ReadyToWork;
        public event EventWorkCompleted WorkCompleted;
        #endregion

        #region[Variables]
        private int workerNumber = 0;
        private Worker[] workers;

        Task task = null;
        CancellationTokenSource cancellationTokenSource;
        private ManualResetEvent _waitSignal = new ManualResetEvent(false);

        private WorkBundle workBundle = null;
        private WorkplaceBundle workplaceBundle = null;

        private bool isPause = false;

        #endregion

        #region[Getter Setter]
        public int WorkerNumber
        {
            get { return this.workerNumber; }
            private set { this.workerNumber = value; }
        }
        #endregion

        public WorkerManager(int workerNumber)
        {
            this.workers = new Worker[workerNumber];
            for (int i = 0; i < workerNumber; i++ )
            {
                this.workers[i] = new Worker();
                this.workers[i].WorkCompleted += WorkCompleted_Callback;
                this.workers[i].ChangedWorkState += ChangedWorkState_Callback;
                this.workers[i].ReadyToWork += ReadyToWork_Callback;
            }

            this.workerNumber = workerNumber;

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            this.task = Task.Factory.StartNew(() => { DoWork(); }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        ~WorkerManager()
        {
            this.isPause = false;
            cancellationTokenSource.Cancel();
            _waitSignal.Set();
        }

        /// <summary>
        /// Inspection 하기전에 작업/작업장/작업리소스 셋팅
        /// </summary>
        public bool SetWork(WorkplaceBundle _workplaceBundle, WorkBundle _workbundle)
        {
            this.workBundle = _workbundle;
            this.workplaceBundle = _workplaceBundle;
            this.workplaceBundle.Reset();


            return true;
        }

        public Worker GetAvailableWorker()
        {
            foreach (Worker worker in workers)
            {
                if (worker.eWorkerState == WORKER_STATE.WORK_ASSIGNED ||
                    worker.eWorkerState == WORKER_STATE.WORKING)
                    continue;

                return worker;
            }

            return null;
        }

        private object LockObj = new object();
        public void AssignWorkToWorker()
        {
            lock(this.LockObj)
            {
                Worker worker = GetAvailableWorker();
                while (worker != null)
                {
                    Workplace workplace = this.workplaceBundle.GetNextWorkplace();
                    if (workplace == null) return;

                    worker.eWorkerState = WORKER_STATE.WORK_ASSIGNED;
                    worker.AssignWork(workplace, this.workBundle);
                    worker.Start();

                    worker = GetAvailableWorker();
                }
            }
        }


        public void ChangedWorkState_Callback(WORK_TYPE work_type, WORKER_STATE worker_state, WORKPLACE_STATE workplace_state, int indexWorkplace , Point SubIndex)
        {
            this.ChangedWorkState(work_type, worker_state, workplace_state, indexWorkplace, SubIndex);
        }

        public void ReadyToWork_Callback(WORK_TYPE work_type, Workplace workplace)
        {
            this.ReadyToWork(work_type, workplace);
        }


        public void WorkCompleted_Callback()
        {
            _waitSignal.Set();

            if (this.WorkCompleted != null)
                this.WorkCompleted();
        }



        #region IWorkable 멤버

        private void DoWork()
        {
            while (true)
            {
                _waitSignal.WaitOne();

                if(isPause == true)
                {
                    _waitSignal.Reset();
                    continue;
                }

                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                AssignWorkToWorker();

                _waitSignal.Reset();
            }
        }
        public void Start()
        {
            this.isPause = false;
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

        #endregion
    }
}
