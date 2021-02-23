#define WORKMANAGER_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;



namespace RootTools_Vision
{
    [Serializable]
    internal class WorkManager : IWorkStartable
    {
        #region [Members]
        private readonly WORK_TYPE workType;
        private readonly WORK_TYPE preWorkType;
        private readonly int workerNumber;
        private readonly bool isFull;

        [NonSerialized]
        private List<Worker> workers;

        [NonSerialized]
        private WORKMANAGER_STATE state = WORKMANAGER_STATE.NONE;

        // Task
        [NonSerialized]
        private Task task = null;

        [NonSerialized]
        private CancellationTokenSource cancellationTokenSource;

        [NonSerialized]
        private ManualResetEvent _waitSignal = new ManualResetEvent(false);

        [NonSerialized]
        private bool isStop = false;

        [NonSerialized]
        WorkBundle workBundle;

        [NonSerialized]
        WorkplaceBundle workplaceBundle;


        #endregion

        #region [Properties]
        public WORK_TYPE WorkType
        {
            get => this.workType;
        }

        public int WorkerNumber
        {
            get => this.workerNumber;
        }

        public bool IsStop
        {
            get => this.isStop;
        }
        public WORKMANAGER_STATE State 
        { 
            get => state; 
            set
            {
                this.state = value;
#if WORKMANAGER_DEBUG
#if DEBUG
                //DebugOutput.PrintWorkManagerInfo(this);
#endif
#endif
            }
        }
        #endregion

        private WorkManager() { }

        /// <summary>
        /// WorkerNumber는 작업을 동시에 수행할 Thread 개수를 말합니다.
        /// </summary>
        /// <param name="_workType">작업 타입</param>
        /// <param name="_preWorkType">작업을 수행하기 위한 개별 workplace의 이전 state</param>
        /// <param name="_workerNumber">Thread 개수</param>
        /// <param name="_isFull">모든 workplace가 _preWorkType일경우에만 작업을 수행할지 여부 </param>
        public WorkManager(WORK_TYPE _workType, WORK_TYPE _preWorkType, int _workerNumber, bool _isFull = false)
        {
            this.workBundle = new WorkBundle();

            this.workType = _workType;
            this.workerNumber = _workerNumber;
            this.preWorkType = _preWorkType;
            this.isFull = _isFull;

            /// Worker들의 모든 동작은 WorkManager에서 일괄 관리함
            cancellationTokenSource = new CancellationTokenSource();

            this.workers = new List<Worker>();
            for(int i=  0; i< this.WorkerNumber; i++)
            {
                Worker wk = new Worker(cancellationTokenSource.Token, i);
                wk.WorkType = this.workType;
                wk.WorkCompleted += WorkCompleted_Callback;
                wk.WorkIncompleted += WorkIncompleted_Callback;
                this.workers.Add(wk);
            }

            WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;


            this.task = Task.Factory.StartNew(() => { Run(); }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            this.isStop = true;

            this.State = WORKMANAGER_STATE.CREATED;
        }

        public WorkManager Clone()
        {
            WorkManager workManager = new WorkManager(this.workType, this.preWorkType, this.workerNumber, this.isFull);
            return workManager;
        }


        public bool SetWorkBundle(WorkBundle bundle)
        {
            this.workBundle.Clear();
            foreach (WorkBase work in bundle)
            {
                if (work.Type == this.workType)
                    this.workBundle.Add(work);
            }
            
            foreach(Worker wk in this.workers)
            {
                wk.SetWorkBundle(this.workBundle.Clone());  //처음에만 복제해서 생성해주고, Workplace만 바꾸기
            }
            return true;
        }

        public bool SetWorkplaceBundle(WorkplaceBundle bundle)
        {
            this.workplaceBundle = bundle; // Copy할 필요없고 WorkFactory에서 카피본을 들고 있는다.
            return true;
        }

        private void WorkIncompleted_Callback(Workplace workplace)
        {
            workplace.IsOccupied = false;
            workplace.WorkState = this.preWorkType;
#if WORKMANAGER_DEBUG
#if DEBUG
            //if (workplace.WorkState == WORK_TYPE.DEFECTPROCESS_WAFER)
            //    DebugOutput.PrintWorkplaceBundle(this.workplaceBundle);
#endif
#endif
            _waitSignal.Set();
        }

        private void WorkCompleted_Callback(Workplace workplace)
        {
            workplace.IsOccupied = false;
            workplace.WorkState = this.workType;
#if WORKMANAGER_DEBUG
#if DEBUG
            //if (workplace.WorkState == WORK_TYPE.DEFECTPROCESS_WAFER)
            //    DebugOutput.PrintWorkplaceBundle(this.workplaceBundle);
#endif
#endif
            _waitSignal.Set();
        }

        private void Run()
        {
            bool exception = false;
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (isStop == true)
                    {
#if WORKMANAGER_DEBUG
#if DEBUG
                        DebugOutput.PrintWorkManagerInfo(this, "STOP");
#endif
#endif
                        Reset();
                        this.State = WORKMANAGER_STATE.STOP;
                        _waitSignal.Reset();
                        _waitSignal.WaitOne();
                    }
                    else
                    {
                        this.State = WORKMANAGER_STATE.READY;
                        _waitSignal.WaitOne(100);
                    }

                    this.State = WORKMANAGER_STATE.CHECK;
                    // 아직 일이 남아있는지 체크
                    if (this.workplaceBundle != null && this.workplaceBundle.CheckStateCompleted(this.workType) == true) // 모두 완료되었다면,
                    {
                        Stop();
                        continue;
                    }

                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        this.State = WORKMANAGER_STATE.EXIT;
                        return;
                    }

                    this.State = WORKMANAGER_STATE.ASSIGN;
                    if (this.workplaceBundle != null)
                    {
                        //Task.Run(() =>
                        {
                            if (AssignWorkToWorker() == false)
                            {
#if WORKMANAGER_DEBUG
#if DEBUG
                                //DebugOutput.PrintWorkManagerInfo(this, "False");
#endif
#endif
                            }
                            else
                            {
#if WORKMANAGER_DEBUG
#if DEBUG
                                //DebugOutput.PrintWorkManagerInfo(this, "True");
#endif
#endif
                            }
                        }
                        //);
                    }


                    this.State = WORKMANAGER_STATE.DONE;


                    _waitSignal.Reset();
                }
            }
            catch(Exception ex)
            {
                exception = true;
                //쓰레드 하나라도 죽으면 WorkFactory Thread 다시 생성하고, WorkFactory Reset

                TempLogger.Write("Worker", ex);
            }
            finally
            {
                this.task = Task.Factory.StartNew(() => { Run(); }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                this.isStop = true;

                this.State = WORKMANAGER_STATE.CREATED;
            }
           
        }



        /// <summary>
        /// Worker를 선택할 때, WorkState를 이 메서드에서 확인안해도 되는 이유는 
        /// 단일 쓰레드이기 때문에 선택된 worker를 다시 할당할 수 없다.
        /// </summary>
        public bool AssignWorkToWorker()
        {
            if (this.isFull == true) // 전체 Workplace State 체크
            {
                if (!this.workplaceBundle.CheckStateCompleted(this.preWorkType))
                    return false;
            }
            
            Worker worker = GetAvailableWorker();
            while (worker != null)
            {

                Workplace workplace = this.workplaceBundle.GetWorkplaceRemained(this.preWorkType);


                if (workplace == null || worker == null)
                {
                    return false;
                }
#if WORKMANAGER_DEBUG
#if DEBUG
                //Debug.WriteLine(this.workType + " : " + workplace.MapIndexX + ", " + workplace.MapIndexY + " : " + "할당");
#endif
#endif

                worker.SetWorkplace(workplace);
                worker.Start();

                worker = GetAvailableWorker();
            }

            return true;
        }


        public Worker GetAvailableWorker()
        {
            // WorkManager의 쓰레드는 하나이므로 lock을 걸거나 동일한 worker를 호출하는 것을 걱정할 필요없음
            foreach(Worker wk in workers)
            {
                if (wk.WorkerState == WORKER_STATE.WORK_ASSIGNED || wk.WorkerState == WORKER_STATE.WORKING)
                    continue;

                return wk;
            }

            return null;
        }

        public void Exit()
        {
            cancellationTokenSource.Cancel();
            this.isStop = false;

            _waitSignal.Set();
            foreach(Worker wk in workers)
            {
                wk.Exit();
            }
            workers.Clear();
        }

        public void Start()
        {
            this.isStop = false;
            _waitSignal.Set();
           
        }

        /// <summary>
        /// Stop 함수에는 Stop 동작만 시키고,
        /// Reset은 루프 안에서 Stop에 걸리면 하도록한다.
        /// </summary>
        public void Stop()
        {
            this.isStop = true;
            foreach (Worker wk in workers)
            {
                wk.Stop();
            }
        }

        private void WorkplaceStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            Workplace workplace = args.workplace;
            if (this.isStop == true || this.state != WORKMANAGER_STATE.READY) return;

            if (this.isFull == true && this.workplaceBundle.CheckStateAll(this.preWorkType) == false) return;

            if (workplace.WorkState == this.preWorkType)
            {
                _waitSignal.Set();
                //Debug.WriteLine("State - " + workplace.WorkState + " : " + workplace.MapIndexX + ", " + workplace.MapIndexY);
            }
        }


        public bool WaitStop()
        {
            Stop();

            foreach(Worker wk in this.workers)
            {
                while (wk.WorkerState != WORKER_STATE.WORK_STOP) Task.Delay(10);
            }

            while (this.State != WORKMANAGER_STATE.STOP) Task.Delay(10);

            return true;
        }

        /// <summary>
        /// 데이터를 해제한다.
        /// </summary>
        //private void Free()
        //{
        //    this.workBundle.Clear();
        //    this.workBundle = null;

        //    this.workplaceBundle.Clear();
        //    this.workplaceBundle = null;

        //    foreach(Worker worker in workers)
        //    {
        //        worker.Stop();
        //    }
        //}


        /// <summary>
        /// 데이터를 초기화한다.
        /// </summary>
        private void Reset()
        {
            this.workBundle.Clear();

            //this.workplaceBundle.Reset();  //새로 시작할때만 workbundle 초기화

            foreach (Worker worker in workers)
            {
                worker.Stop();
            }
        }
    }
}
