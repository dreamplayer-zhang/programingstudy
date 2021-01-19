using RootTools;
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
    internal enum WORKER_STATE
    {
        NONE = 0,
        WORK_ASSIGNED = 1,
        WORKING = 2,
        WORK_COMPLETED = 3
    }

    public delegate void EventWorkCompleted(Workplace obj);

    sealed internal class Worker : IWorkStartable
    {
        #region [Members]

        private readonly int workerIndex;
        private readonly CancellationToken token;

        private ManualResetEvent _waitSignal = new ManualResetEvent(false);
        private Task task = null;
        private bool isStop = false;
        private WORKER_STATE workerState;
        private WORK_TYPE workType;
        private WorkBundle works;
        private Workplace currentWorkplace;

        public event EventWorkCompleted WorkCompleted;
        public event EventWorkCompleted WorkIncompleted;

        private byte[] workplaceBufferR_GRAY;
        private byte[] workplaceBufferG;
        private byte[] workplaceBufferB;

        //private bool isUseWorkplaceBuffer;
        #endregion

        #region [Properties]

        public bool IsStop
        {
            get => this.isStop;
        }

        public WORKER_STATE WorkerState
        {
            get { return this.workerState; }
        }
        public WORK_TYPE WorkType
        {
            get => this.workType;
            set => this.workType = value;
        }


        public Workplace Workplace
        {
            get => this.currentWorkplace;
        }

        public int WorkerIndex
        {
            get => this.workerIndex;
        }
        #endregion

        public Worker(CancellationToken _token, int index)
        {
            this.token = _token;
            this.workerIndex = index;
            this.task = Task.Factory.StartNew(() => { Run(); }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current); // 짧은 작업이 아닌 경우 LongRunning 옵션을 반드시 사용해야함. 자세한 것은 검색
        }

        ~Worker()
        {

        }


        public void SetWorkBundle(WorkBundle works)
        {
            if (works == null)
                throw new ArgumentException("Worker에 Workbundle 설정에 실패하였습니다.(works == null)");

            this.works = works;
        }

        public void SetWorkplace(Workplace workplace)
        {
            this.works.SetWorkplace(workplace);
            this.currentWorkplace = workplace;
            this.workerState = WORKER_STATE.WORK_ASSIGNED;

            if(this.workType == WORK_TYPE.INSPECTION)
            {
                // R_GRAY
                if (this.workplaceBufferR_GRAY != null && (this.workplaceBufferR_GRAY.Length == workplace.Width * workplace.Height))
                    return;
                else
                {
                    this.workplaceBufferR_GRAY = new byte[workplace.Width * workplace.Height];
                }

                // G
                if (this.workplaceBufferG != null && (this.workplaceBufferG.Length == workplace.Width * workplace.Height))
                    return;
                else
                {
                    this.workplaceBufferG = new byte[workplace.Width * workplace.Height];
                }

                // B
                if (this.workplaceBufferB != null && (this.workplaceBufferB.Length == workplace.Width * workplace.Height))
                    return;
                else
                {
                    this.workplaceBufferB = new byte[workplace.Width * workplace.Height];
                }
            }
        }

        private void Run()
        {
            bool exception = false;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    //if (this.workerState == WORKER_STATE.NONE || this.workerState == WORKER_STATE.WORK_COMPLETED)
                    //{
                    //    this.workerState = WORKER_STATE.NONE;
                    //    _waitSignal.WaitOne();
                    //}
                    if(this.workerState != WORKER_STATE.WORK_ASSIGNED)
                        _waitSignal.WaitOne();

                    if (this.isStop == true)
                    {
                        Reset();
                        _waitSignal.Reset();
                        _waitSignal.WaitOne();
                    }

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    // Workplace Copy
                    if (this.workType == WORK_TYPE.INSPECTION && this.workerState != WORKER_STATE.WORKING) // 이미 Working State면 Copy가 되어 있는 상태
                    {
                        CopyWorkplaceBuffer();

                        this.works.SetWorkplaceBuffer(this.workplaceBufferR_GRAY, this.workplaceBufferG, this.workplaceBufferB);
                    }

                    //// Work Start ////
                    this.workerState = WORKER_STATE.WORKING;

                    bool workDone = false;

                    foreach (WorkBase work in this.works)
                    {
                        if(work.IsPreworkDone == false)
                            work.DoPrework();

                        if (work.IsPreworkDone == true) // Prework Done(0)
                        {
                            if (work.IsWorkDone == true)  // 이미 작업을 한 경우 다음 work올 넘어감
                                continue;
                            else
                            {
#if DEBUG
                                DebugOutput.PrintWork(work);
#endif
                                work.DoWork();
                                workDone = true;
                            }
                        }
                        else
                        {
                            workDone = false;
                            break;
                        }
                    }

                    _waitSignal.Reset();

                    if (workDone == false && this.works.Count != 0)
                    {
#if DEBUG
                        DebugOutput.PrintWorker(this, "Incomplete");                        
#endif
                        Incomplete();
                    }
                    else
                    {
#if DEBUG
                        DebugOutput.PrintWorker(this, "Complete");
#endif
                        Complete();
                    }
                }
            }
            catch(Exception ex)
            {
                exception = true;
                //쓰레드 하나라도 죽으면 WorkFactory Thread 다시 생성하고, WorkFactory Reset
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if(exception == true)
                {

                }
            }
        }

        private void Incomplete()
        {
            if (this.WorkIncompleted != null)
                this.WorkIncompleted(this.currentWorkplace);

            this.currentWorkplace = null;
            this.works.Reset();

            this.workerState = WORKER_STATE.WORK_COMPLETED;
        }

        private void Complete()
        {
            if (this.WorkCompleted != null)
                this.WorkCompleted(this.currentWorkplace);

            this.currentWorkplace = null;
            this.works.Reset();

            this.workerState = WORKER_STATE.WORK_COMPLETED;
        }

        /// <summary>
        /// 모든 데이터를 초기화한다.
        /// 다른곳에서 참조하는 데이터가 있을 수 있으므로
        /// 루프의 Stop 에서만 동작하게 한다.
        /// </summary>
        private void Reset()
        {
            this.currentWorkplace = null;

            //this.works.Clear();
            //this.works = null;

            this.workerState = WORKER_STATE.NONE;

            this.workplaceBufferR_GRAY = null;
            this.workplaceBufferG = null;
            this.workplaceBufferB = null;
        }

        public void Exit()
        {
            this.isStop = false;
            _waitSignal.Set();
        }

        public void Start()
        {
            this.isStop = false;
            _waitSignal.Set();
        }

        public void Stop()
        {
            // 여기서 초기화 해주면 검사 도중 죽을 수 있어서 루프에서 Stop에 걸렸을 때 초기화 해줘야함
            this.isStop = true;
        }

        private void CopyWorkplaceBuffer()
        {
            if (this.currentWorkplace.Width == 0 || this.currentWorkplace.Height == 0) return;

            Tools.ParallelImageCopy(
                this.currentWorkplace.SharedBufferR_GRAY,
                this.currentWorkplace.SharedBufferWidth,
                this.currentWorkplace.SharedBufferHeight,
                new CRect
                (
                    this.currentWorkplace.PositionX,
                    this.currentWorkplace.PositionY + this.currentWorkplace.Height,
                    this.currentWorkplace.PositionX + this.currentWorkplace.Width,
                    this.currentWorkplace.PositionY),
                this.workplaceBufferR_GRAY);


            if (this.currentWorkplace.SharedBufferByteCnt == 3)
            {
                Tools.ParallelImageCopy(
                    this.currentWorkplace.SharedBufferG,
                    this.currentWorkplace.SharedBufferWidth,
                    this.currentWorkplace.SharedBufferHeight,
                    new CRect
                    (
                        this.currentWorkplace.PositionX,
                        this.currentWorkplace.PositionY + this.currentWorkplace.Height,
                        this.currentWorkplace.PositionX + this.currentWorkplace.Width,
                        this.currentWorkplace.PositionY),
                    this.workplaceBufferG);

                Tools.ParallelImageCopy(
                    this.currentWorkplace.SharedBufferB,
                    this.currentWorkplace.SharedBufferWidth,
                    this.currentWorkplace.SharedBufferHeight,
                    new CRect
                    (
                        this.currentWorkplace.PositionX,
                        this.currentWorkplace.PositionY + this.currentWorkplace.Height,
                        this.currentWorkplace.PositionX + this.currentWorkplace.Width,
                        this.currentWorkplace.PositionY),
                    this.workplaceBufferB);
            }
        }
    }
}
