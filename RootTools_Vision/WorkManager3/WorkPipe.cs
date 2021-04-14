using RootTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision.WorkManager3
{
    public class WorkPipe
    {

        private WorkPipe nextPipe = null;

        List<WorkBase> workList;
        ConcurrentQueue<Workplace> queue;

        TaskManager<Workplace> taskManager;

        private readonly WORK_TYPE type;

        private int workplaceTotalCount = 0;
        private int maxThreadNum = 1;
        private readonly bool isWaitAll;

        private Task task;

        private List<ColorBuffer> bufferList;

        #region [Properties]

        public bool IsReady
        {
            get
            {
                return (IsRunning == false) && (this.taskManager.CompleteCount == 0);
            }
        }
        public bool IsRunning
        {
            get;
            private set;
        }

        private bool IsStopRequest
        {
            get;
            set;
        }

        #endregion

        #region [ColorBuffer]
        private class ColorBuffer
        {
            int width;
            int height;
            byte[] bufferR_GRAY;
            byte[] bufferG;
            byte[] bufferB;

            public byte[] BufferR_GRAY 
            { 
                get => bufferR_GRAY;
                set => bufferR_GRAY = value; 
            }
            public byte[] BufferG 
            { 
                get => bufferG; 
                set => bufferG = value; 
            }
            public byte[] BufferB 
            { 
                get => bufferB; 
                set => bufferB = value; 
            }
            public int Width {
                get => width;
                set => width = value; 
            }
            public int Height 
            { 
                get => height; 
                set => height = value; 
            }

            public ColorBuffer()
            {
                this.width = 0;
                this.height = 0;
            }

            public ColorBuffer(int width, int height)
            {
                this.width = width;
                this.height = height;

                BufferR_GRAY = new byte[width * height];
                BufferG = new byte[width * height];
                BufferB = new byte[width * height];
            }

            public void Realloc(int width, int height)
            {
                this.width = width;
                this.height = height;

                BufferR_GRAY = new byte[width * height];
                BufferG = new byte[width * height];
                BufferB = new byte[width * height];
            }
        }
        #endregion

        public WorkPipe(WORK_TYPE type, int threadNum = 1, bool isWaitAll = false)
        {
            this.workList = new List<WorkBase>();
            this.queue = new ConcurrentQueue<Workplace>();

            this.type = type;
            this.maxThreadNum = threadNum;
            this.isWaitAll = isWaitAll;

            this.taskManager = new TaskManager<Workplace>(threadNum);
            this.taskManager.TaskCompleted += TaskCompleted_Callback;


            // task 개수랑 같으니깐 task 인덱스로?
            this.bufferList = new List<ColorBuffer>();
            this.bufferList.Add(new ColorBuffer(1, 1));
        }

        public void SetNextPipe(WorkPipe pipe)
        {
            this.nextPipe = pipe;
        }

        public void Initialize(WorkBundle works, int workplaceCount)
        {
            ClearQueue();

            Debug.WriteLine(type.ToString() + " : " + this.queue.Count);

            this.workList = (from work in works
                             where work.Type == this.type
                             select work).ToList();

            this.workplaceTotalCount = workplaceCount;
        }

        public void ClearQueue()
        {
            this.taskManager.Clear();

            Workplace temp;
            while (this.queue.TryDequeue(out temp) || !this.queue.IsEmpty) ;
        }


        public bool Start(CancellationToken token)
        {
            // task가 실행 중인 경우 다시 실행되지 않도록
            if (this.IsRunning == true) return false;

            if(this.task != null && (this.task.IsCompleted || this.task.IsFaulted || this.task.IsCanceled))
            {
                this.IsStopRequest = false;
                this.task = Task.Factory.StartNew(Loop, token);
                return true;
            }
            else if(this.task == null)
            {
                this.IsStopRequest = false;
                this.task = Task.Factory.StartNew(Loop, token);
                return true;
            }

            return false;
        }

        private void Loop(object obj)
        {
            this.IsRunning = true;

            CancellationToken token = (CancellationToken)obj;

            int count = 0;
            bool retry = false;
            Workplace workplace = new Workplace();

            if(this.isWaitAll == true)
            {
                while (this.queue.Count != this.workplaceTotalCount && !token.IsCancellationRequested) 
                    Thread.Sleep(100);
            }

            while (count < this.workplaceTotalCount && !token.IsCancellationRequested)
            {
                if (this.taskManager.IsAvailableTask) // ConcurrentQueue 이기 때문에 count에 접근할 필요없음
                {
                    if(retry == true)
                    {
                        Debug.WriteLine(type.ToString() + " : " + "Retry (" + workplace.MapIndexX.ToString() + "," + workplace.MapIndexY + ") " + count + " " + this.taskManager.TaskCount);
                        retry = false;
                        if (this.taskManager.Invoke(CreateJob(workplace, this.workList, token)))
                        {
                            count++;
                            Debug.WriteLine(type.ToString() + " : " + "Dequeue (" + workplace.MapIndexX.ToString() + "," + workplace.MapIndexY + ") " + count + " " + this.taskManager.TaskCount);
                        }
                        else
                        {
                            retry = true;
                            //MessageBox.Show("Fail");
                        }
                    }
                    else
                    {
                        if (queue.TryDequeue(out workplace) && !token.IsCancellationRequested && !this.IsStopRequest)  // TryQueue가 실패할 경우가 있음
                        {
                            if (this.taskManager.Invoke(CreateJob(workplace, this.workList, token)))
                            {
                                count++;
                                Debug.WriteLine(type.ToString() + " : " + "Dequeue (" + workplace.MapIndexX.ToString() + "," + workplace.MapIndexY + ") " + count + " " + this.taskManager.TaskCount);
                            }
                            else
                            {
                                retry = true;
                                //MessageBox.Show("Fail");
                            }
                        }
                    }
                }
            }

            Debug.WriteLine("End " + type.ToString() + " " + count);

            this.IsRunning = false;
        }


        public bool TryStop(int timeout = 1000)
        {
            this.IsStopRequest = true;
            Debug.WriteLine("TryStop : " + type.ToString());


            if (this.task == null)
                return true;
            else if(this.IsRunning == true)
            {
                bool result = true;
                this.workplaceTotalCount = 0;
                result = this.task.Wait(timeout);
                ClearQueue();
                return result;
            }

            return true;
        }

        public void SetQueue(ConcurrentQueue<Workplace> queue)
        {
            this.queue = queue;
        }

        private void Enqueue(Workplace workplace)
        {
            this.queue.Enqueue(workplace);
            //Debug.WriteLine(type.ToString() + " : " + "Enqueue(" + workplace.MapIndexX.ToString() + "," + workplace.MapIndexY + ")");
        }

        private void TaskCompleted_Callback(Workplace workplace)
        {
            workplace.WorkState = type;

            if (this.nextPipe != null && IsStopRequest == false)
                this.nextPipe.Enqueue(workplace);

            this.taskManager.DecreaseCount();
            Debug.WriteLine(type.ToString() + " : " + "Completed (" + workplace.MapIndexX.ToString() + "," + workplace.MapIndexY + ")");
        }


        private static Task<Workplace> CreateJob(Workplace workplace, List<WorkBase> works, CancellationToken token)
        {
            return new Task<Workplace>(() =>
            {
                ColorBuffer buffer = new ColorBuffer();
                if (works.Count > 0 && works[0].Type == WORK_TYPE.INSPECTION)
                {
                    buffer.Realloc(workplace.Width, workplace.Height);

                    CopyBuffer(workplace, ref buffer);
                }

                foreach (WorkBase work in works)
                {
                    // 파라매터/레시피는 Clone으로 Copy되고...
                    // 파라매터/레시피는 Inspection class 안에 존재해야함

                    WorkBase clone = work.Clone();

                    if(work.Type == WORK_TYPE.INSPECTION)
                    {
                        clone.SetWorkplaceBuffer(buffer.BufferR_GRAY, buffer.BufferG, buffer.BufferB);
                    }
                    
                    clone.SetWorkplace(workplace);
                    clone.SetWorkplaceBundle(workplace.ParentBundle);

                    while (!clone.DoPrework() && !token.IsCancellationRequested)
                    {
                        Thread.Sleep(10);
                    }

                    while (!clone.DoWork() && !token.IsCancellationRequested)
                    {
                        Thread.Sleep(10);
                    }                    
                }

                //Debug.WriteLine(workplace.WorkState.ToString() + " : " + "Task (" + workplace.MapIndexX.ToString() + "," + workplace.MapIndexY + ") " + count);

                return workplace;
                //return result;
            });
        }


        /// <summary>
        /// 멀티쓰레딩 테스트용
        /// </summary>
        private static Task<Workplace> CreateTaskTest(Workplace workplace, List<WorkBase> works)
        {
            return new Task<Workplace>(() =>
            {
                foreach(WorkBase work in works)
                {
                    // 파라매터/레시피는 Clone으로 Copy되고...
                    // 파라매터/레시피는 Inspection class 안에 존재해야함

                    WorkBase clone = work.Clone();
                    
                    clone.SetWorkplace(workplace);
                    //clone.DoPrework();
                    //clone.DoWork();
                }

                //Debug.WriteLine(workplace.WorkState.ToString() + " : " + "Task (" + workplace.MapIndexX.ToString() + "," + workplace.MapIndexY + ") " + count);

                return workplace;
                //return result;
            });
        }

        private static void CopyBuffer(Workplace workplace, ref ColorBuffer buffer)
        {
            if (workplace.Width == 0 || workplace.Height == 0) return;

            if (workplace.SharedBufferInfo.PtrR_GRAY == IntPtr.Zero)
                return;

            Tools.ParallelImageCopy(
                workplace.SharedBufferInfo.PtrR_GRAY,
                workplace.SharedBufferInfo.Width,
                workplace.SharedBufferInfo.Height,
                new CRect
                (
                    workplace.PositionX,
                    workplace.PositionY + workplace.Height,
                    workplace.PositionX + workplace.Width,
                    workplace.PositionY),
                buffer.BufferR_GRAY);


            if (workplace.SharedBufferInfo.PtrG == IntPtr.Zero ||
               workplace.SharedBufferInfo.PtrB == IntPtr.Zero)
            {
                return;
            }

            if (workplace.SharedBufferInfo.ByteCnt == 3)
            {
                Tools.ParallelImageCopy(
                    workplace.SharedBufferInfo.PtrG,
                    workplace.SharedBufferInfo.Width,
                    workplace.SharedBufferInfo.Height,
                    new CRect
                    (
                        workplace.PositionX,
                        workplace.PositionY + workplace.Height,
                        workplace.PositionX + workplace.Width,
                        workplace.PositionY),
                    buffer.BufferG);

                Tools.ParallelImageCopy(
                    workplace.SharedBufferInfo.PtrB,
                    workplace.SharedBufferInfo.Width,
                    workplace.SharedBufferInfo.Height,
                    new CRect
                    (
                        workplace.PositionX,
                        workplace.PositionY + workplace.Height,
                        workplace.PositionX + workplace.Width,
                        workplace.PositionY),
                   buffer.BufferB);
            }
        }
    }
}
