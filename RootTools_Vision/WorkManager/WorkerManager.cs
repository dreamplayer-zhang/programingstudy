using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace RootTools_Vision
{
    public class WorkerManager : IWorkable
    {
        List<Worker> workers;

        List<WorkBundle> workBundleList;

        WorkBundle workBundle;
        WorkplaceBundle workplaceBundle;

        STATE_CHECK_TYPE eStateCheckType;

        Task task = null;
        CancellationTokenSource cancellationTokenSource;
        private ManualResetEvent _waitSignal = new ManualResetEvent(false);

        private bool isStop = false;

        WORK_TYPE resultState = WORK_TYPE.NONE;
        WORK_TYPE excuteCondition = WORK_TYPE.NONE;


        private int m_nLine = 0;

        public WorkerManager(List<Worker> _workers, WORK_TYPE _resultState, WORK_TYPE _excuteCondition, STATE_CHECK_TYPE _state_check_type = STATE_CHECK_TYPE.CHIP)
        {
            this.eStateCheckType = _state_check_type;
            foreach (Worker worker in _workers)
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
            this.workBundleList = new List<WorkBundle>();
            this.workBundleList.Clear();

            foreach (Workplace wp in this.workplaceBundle)
            {
                this.workBundle.Workplace = wp;
                this.workBundle.WorkplaceBundle = this.workplaceBundle;
                this.workBundleList.Add(workBundle.Clone());
            }
        }

        private void WorkplaceStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            Workplace workplace = args.workplace;
            if(workplace.STATE == this.excuteCondition)
            {
                _waitSignal.Set();
            }
        }

        private void WorkCompleted_Callback(object obj)
        {
            if (eStateCheckType == STATE_CHECK_TYPE.WAFER)
            {
                if (this.workplaceBundle != null)
                {
                    this.workplaceBundle.SetStateAll(this.resultState);
                }
            }
            else
            {
                Workplace workplace = obj as Workplace;
                workplace.STATE = this.resultState;
            }
            
            _waitSignal.Set();
        }

        public Worker GetAvailableWorker()
        {
            foreach (Worker worker in workers)
            {
                if (worker.eWorkerState == WORKER_STATE.WORK_ASSIGNED ||
                    worker.eWorkerState == WORKER_STATE.WORKING)
                    continue;

                worker.eWorkerState = WORKER_STATE.WORK_ASSIGNED;
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
                    if (this.isStop == true)
                    {
                        worker.eWorkerState = WORKER_STATE.NONE;
                        return;
                    }

                    if (this.workplaceBundle.CheckStateAll(WORK_TYPE.DEFECTPROCESS_WAFER) == true)
                    {
                        worker.eWorkerState = WORKER_STATE.NONE;
                        break;
                    }

                    if (eStateCheckType == STATE_CHECK_TYPE.WAFER)
                    {
                        if (this.workplaceBundle == null) continue;

                        if(this.workplaceBundle.CheckStateAll(this.excuteCondition) == true)
                        {
                            worker.eWorkerState = WORKER_STATE.WORK_ASSIGNED;
                            this.workplaceBundle[0].IsOccupied = true;
                            this.workBundle.Workplace = this.workplaceBundle[0];
                            this.workBundle.WorkplaceBundle = this.workplaceBundle;
                            worker.SetWorkBundle(this.workBundle.Clone());
                            worker.Start();

                            worker = GetAvailableWorker();
                        }
                        else
                        {
                            worker.eWorkerState = WORKER_STATE.NONE;
                        }
                    }
                    else
                    {
                        Workplace workplace = this.workplaceBundle.GetWorkplaceByState(this.excuteCondition);
                        if (workplace == null)
                        {
                            worker.eWorkerState = WORKER_STATE.NONE;
                            return;
                        }

                        workplace.IsOccupied = true;

                        worker.SetWorkBundle(this.workBundleList[workplace.Index]);
                        worker.Start();

                        worker = GetAvailableWorker();
                    }
                }
            }
        }

        private void Run()
        {
            while (true)
            {
                _waitSignal.WaitOne();

                if (isStop == true)
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
            WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;

            this.isStop = false;
            _waitSignal.Set();
        }

        public void Stop()
        {
            WorkEventManager.WorkplaceStateChanged -= WorkplaceStateChanged_Callback;

            this.isStop = true;

            if(this.workplaceBundle != null)
                this.workplaceBundle.Reset();

            this.workBundle = null;
            this.workplaceBundle = null;

        }
    }
}
