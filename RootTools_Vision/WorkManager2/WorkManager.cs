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
    sealed internal class WorkManager : IWorkStartable
    {
        #region [Members]
        private readonly WORK_TYPE workType;
        private readonly WORK_TYPE preWorkType;
        private readonly int workerNumber;
        private readonly bool isFull;

        private List<Worker> workers;


        // Task
        private Task task = null;
        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEvent _waitSignal = new ManualResetEvent(false);

        private bool isStop = false;

        WorkBundle workBundle;
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
                Worker wk = new Worker(cancellationTokenSource.Token);
                wk.WorkType = this.workType;
                wk.WorkCompleted += WorkCompleted_Callback;
                this.workers.Add(wk);
            }

            WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;


            this.task = Task.Factory.StartNew(() => { Run(); }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
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

        private void WorkCompleted_Callback(Workplace workplace)
        {
            workplace.IsOccupied = false;
            workplace.WorkState = this.workType;
            _waitSignal.Set();
        }

        private void Run()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                _waitSignal.WaitOne();

                if (isStop == true)
                {
                    Reset();
                    _waitSignal.Reset();
                    continue;
                }

                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                // 아직 일이 남아있는지 체크
                if(this.workplaceBundle.CheckStateCompleted(this.workType) == true) // 모두 완료되었다면,
                {
                    Stop();
                }
                else
                {
                    if(AssignWorkToWorker() == false)
                    {

                    }
                }

                

                _waitSignal.Reset();
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
                if (!this.workplaceBundle.CheckStateAll(this.preWorkType))
                    return false;
            }

            Worker worker = GetAvailableWorker();
            while(worker != null)
            {
                
                Workplace workplace = this.workplaceBundle.GetWorkplaceRemained(this.preWorkType);

                
                if (workplace == null || worker == null)
                {
                    return false;
                }

                Debug.WriteLine(this.workType +" : " + workplace.MapIndexX + ", " + workplace.MapIndexY + " : " + "할당");

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

            foreach(Worker wk in workers)
            {
                wk.Stop();
            }
        }

        private void WorkplaceStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            if (this.isStop == true) return;

            Workplace workplace = args.workplace;
            if (workplace.WorkState == this.preWorkType)
            {
                _waitSignal.Set();
                //Debug.WriteLine("State - " + workplace.WorkState + " : " + workplace.MapIndexX + ", " + workplace.MapIndexY);
            }
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

            this.workplaceBundle.Reset();

            foreach (Worker worker in workers)
            {
                worker.Stop();
            }
        }
    }
}
