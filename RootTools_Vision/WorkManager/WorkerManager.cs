using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkerManager : IWorkable
    {
        List<Worker> workers;

        WorkBundle workBundle;
        WorkplaceBundle workplaceBundle;

        Task task = null;
        CancellationTokenSource cancellationTokenSource;
        private ManualResetEvent _waitSignal = new ManualResetEvent(false);

        private bool isPause = false;

        WORKPLACE_STATE resultState = WORKPLACE_STATE.NONE;
        WORKPLACE_STATE excuteCondition = WORKPLACE_STATE.NONE;


        public WorkerManager(List<Worker> _workers, WORKPLACE_STATE _resultState, WORKPLACE_STATE _excuteCondition)
        {
            foreach(Worker worker in _workers)
            {
                worker.WorkCompleted += WorkCompleted_Callback;
            }
            this.workers = _workers;

            this.resultState = _resultState;
            this.excuteCondition = _excuteCondition;

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            this.task = Task.Factory.StartNew(() => { Run(); }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void SetBundles(WorkBundle _workbundle, WorkplaceBundle _workplacebundle)
        {
            this.workBundle = _workbundle;
            this.workplaceBundle = _workplacebundle;
        }

        private void WorkplaceStateChanged_Callback(object obj)
        {
            Workplace workplace = obj as Workplace;
            if(workplace.STATE == this.excuteCondition)
            {
                _waitSignal.Set();
            }
        }


        private void WorkCompleted_Callback(object obj)
        {
            Workplace workplace = obj as Workplace;
            workplace.STATE = this.resultState;
            
            
            _waitSignal.Set();
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
            lock (this.LockObj)
            {
                Worker worker = GetAvailableWorker();
                while (worker != null)
                {
                    Workplace workplace = this.workplaceBundle.GetWorkplaceByState(this.excuteCondition);
                    if (workplace == null) return;

                    this.workBundle.Workplace = workplace;

                    worker.eWorkerState = WORKER_STATE.WORK_ASSIGNED;
                    worker.SetWorkBundle(this.workBundle);
                    worker.Start();

                    worker = GetAvailableWorker();
                }
            }
        }

        private void Run()
        {
            while (true)
            {
                _waitSignal.WaitOne();

                if (isPause == true)
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
            // event
            this.workplaceBundle.WorkplaceStateChanged += WorkplaceStateChanged_Callback;

            this.isPause = false;
            _waitSignal.Set();
        }

        public void Pause()
        {
            this.workplaceBundle.WorkplaceStateChanged -= WorkplaceStateChanged_Callback;

            this.isPause = true;
        }

        public void Stop()
        {
            this.workplaceBundle.WorkplaceStateChanged -= WorkplaceStateChanged_Callback;

            this.isPause = true;
        }
    }
}
