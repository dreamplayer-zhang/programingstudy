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

    public delegate void WorkPipeDoneHandler();

    public class WorkPipe
    {
        public event WorkPipeDoneHandler WorkPipeDone;

        private WorkPipe nextPipe = null;

        List<WorkBase> workList;
        ConcurrentQueue<Workplace> queue;

        TaskManager<Workplace> taskManager;

        CancellationTokenSource cancelTokenSrc;

        private readonly WORK_TYPE type;

        private int workplaceTotalCount = 0;
        private int maxThreadNum = 1;
        private readonly bool isWaitAll;

        private Task task;

        private CopyBufferData[] copyBufferList;

        private bool bCopyBuffer = false;

        #region [Properties]

        public bool IsReady
        {
            get
            {
                return (IsRunning == false) /*&& (this.taskManager.CompleteCount == 0)*/;
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
        private class CopyBufferData
        {
            int width;
            int height;
            byte[] bufferR_GRAY;
            byte[] bufferG;
            byte[] bufferB;

            List<byte[]> bufferList;

            int count;

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
            public int Width
            {
                get => width;
                set => width = value;
            }
            public int Height
            {
                get => height;
                set => height = value;
            }

            public bool IsAssigned
            {
                get;
                set;
            }

            public List<byte[]> BufferList
            {
                get => this.bufferList;
            }

            public CopyBufferData()
            {
                this.width = 0;
                this.height = 0;
                this.bufferList = new List<byte[]>();
                this.IsAssigned = false;
            }

            public CopyBufferData(int w, int h, int cnt)
            {
                this.width = w;
                this.height = h;
                this.count = cnt;

                this.bufferList = new List<byte[]>();
                for (int i = 0; i < cnt; i++)
                {
                    this.bufferList.Add(new byte[w * h]);

                    if (i == 0) BufferR_GRAY = this.bufferList[0];
                    if (i == 1) BufferG = this.bufferList[1];
                    if (i == 2) BufferB = this.bufferList[2];
                }

                this.IsAssigned = false;
            }

            public void Realloc(int w, int h, int cnt)
            {
                this.width = w;
                this.height = h;
                this.count = cnt;

                this.bufferList = new List<byte[]>();
                for (int i = 0; i < cnt; i++)
                {
                    this.bufferList.Add(new byte[w * h]);

                    if (i == 0) BufferR_GRAY = this.bufferList[0];
                    if (i == 1) BufferG = this.bufferList[1];
                    if (i == 2) BufferB = this.bufferList[2];
                }

                this.IsAssigned = false;
            }

            public void Realloc(int w, int h)
            {
                if (this.width != w || this.height != h)
                {
                    this.width = w;
                    this.height = h;

                    this.bufferList.Clear();
                    for (int i = 0; i < this.count; i++)
                    {
                        this.bufferList.Add(new byte[w * h]);

                        if (i == 0) BufferR_GRAY = this.bufferList[0];
                        if (i == 1) BufferG = this.bufferList[1];
                        if (i == 2) BufferB = this.bufferList[2];
                    }
                }
                this.IsAssigned = false;
            }
        }
        #endregion

        public WorkPipe(WORK_TYPE type, int threadNum = 1, bool isWaitAll = false, bool bCopyBuffer = false)
        {
            this.workList = new List<WorkBase>();
            this.queue = new ConcurrentQueue<Workplace>();

            this.type = type;
            this.maxThreadNum = threadNum;
            this.isWaitAll = isWaitAll;

            this.bCopyBuffer = bCopyBuffer;

            this.taskManager = new TaskManager<Workplace>(threadNum);

            if (this.type == WORK_TYPE.INSPECTION)
            {
                this.copyBufferList = new CopyBufferData[maxThreadNum];
                for (int i = 0; i < this.copyBufferList.Length; i++)
                    this.copyBufferList[i] = new CopyBufferData();
            }

            //WorkPipeDone += WorkPipeDone_Callback;

            // task 개수랑 같으니깐 task 인덱스로?
            //this.copybufferList = new CopyBufferData[threadNum];
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


        public bool Start(CancellationTokenSource tokenSrc)
        {
            // task가 실행 중인 경우 다시 실행되지 않도록
            if (this.IsRunning == true) return false;

            if (this.task != null && (this.task.IsCompleted || this.task.IsFaulted || this.task.IsCanceled))
            {
                this.taskManager.TaskCompleted += TaskCompleted_Callback;
                this.IsStopRequest = false;
                this.task = Task.Factory.StartNew(Loop, tokenSrc.Token);
                this.cancelTokenSrc = tokenSrc;
                return true;
            }
            else if (this.task == null)
            {
                this.taskManager.TaskCompleted += TaskCompleted_Callback;
                this.IsStopRequest = false;
                this.task = Task.Factory.StartNew(Loop, tokenSrc.Token);
                this.cancelTokenSrc = tokenSrc;

                return true;
            }

            return false;
        }

        public void Exit()
        {
            if (task != null)
            {
                //task.Dispose();
            }
        }

        private CopyBufferData GetAvailableCopyBuffer()
        {
            foreach(CopyBufferData buffer in this.copyBufferList)
            {
                if(buffer.IsAssigned == false)
                {
                    buffer.IsAssigned = true;
                    return buffer;
                }
            }

            return null;
        }

        private void Loop(object obj)
        {
            this.IsRunning = true;

            CancellationToken token = (CancellationToken)obj;

            try
            {
                using (token.Register(Thread.CurrentThread.Abort, false))
                {
                    int count = 0;
                    Workplace workplace = new Workplace();

                    if (this.isWaitAll == true)
                    {
                        while (this.queue.Count != this.workplaceTotalCount && !token.IsCancellationRequested)
                            Thread.Sleep(100);
                    }

                    while (count < this.workplaceTotalCount && !token.IsCancellationRequested)
                    {
                        if (!this.taskManager.IsAvailableTask) continue;

                        if (queue.TryDequeue(out workplace) && !token.IsCancellationRequested && !this.IsStopRequest)
                        {
                            CopyBufferData copyBuffer = null;
                            if (this.type == WORK_TYPE.INSPECTION && this.bCopyBuffer == true)
                            {
                                while ((copyBuffer = GetAvailableCopyBuffer()) == null && !token.IsCancellationRequested)
                                {
                                    Thread.Sleep(100);
                                };
                            }

                            while (!this.taskManager.Invoke(DoJob(workplace, this.workList, token, copyBuffer, this.bCopyBuffer)) && !token.IsCancellationRequested)
                            {
                                Thread.Sleep(100);
                            }
                            count++;
                        }
                    }

                    this.IsRunning = false;
                }
            }
            catch(ThreadAbortException)
            {
                this.IsRunning = false;
            }
            finally
            {
                if (this.WorkPipeDone != null)
                    WorkPipeDone();
            }
        }

        public void Abort()
        {
            if (this.cancelTokenSrc != null)
                this.cancelTokenSrc.Cancel();

            this.cancelTokenSrc = null;
        }


        public bool TryStop(int timeout = 1000)
        {
            this.IsStopRequest = true;
            Debug.WriteLine("TryStop : " + type.ToString());

            this.taskManager.TaskCompleted -= TaskCompleted_Callback;

            if (this.task == null)
                return true;
            else if (this.IsRunning == true)
            {
                bool result = true;
                this.workplaceTotalCount = 0;

                ClearQueue();
                result = this.task.Wait(timeout);
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
        }

        private void TaskCompleted_Callback(Workplace workplace)
        {
            workplace.WorkState = type;

            Debug.WriteLine(workplace.MapIndexX + "," + workplace.MapIndexY + " : " + workplace.WorkState + " -> " + type.ToString());

            if (this.nextPipe != null && IsStopRequest == false)
                this.nextPipe.Enqueue(workplace);
        }


        private static Task<Workplace> DoJob(Workplace workplace, List<WorkBase> works, CancellationToken token, CopyBufferData copyBuffer = null, bool bCopyBuffer = false)
        {
            return new Task<Workplace>(() =>
            {
                try
                {
                    using(token.Register(Thread.CurrentThread.Abort))
                    {
                        //CopyBuffer buffer = null;
                        if (works.Count > 0 && works[0].Type == WORK_TYPE.INSPECTION && bCopyBuffer == true)
                        {
                            //buffer.Realloc(workplace.Width, workplace.Height);

                            CopyBuffer(workplace, ref copyBuffer);
                        }

                        foreach (WorkBase work in works)
                        {
                            WorkBase clone = work.Clone();

                            if (work.Type == WORK_TYPE.INSPECTION && copyBuffer != null)
                            {
                                clone.SetWorkplaceBuffer(copyBuffer.BufferList);
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

                        if (copyBuffer != null)
                        {
                            copyBuffer.IsAssigned = false;
                        }
                    }
                }
                catch(Exception ex)
                {
                    //MessageBox.Show("Error");

                    TempLogger.Write("Worker", ex);
                }
                finally
                {

                }

                return workplace;
            });
        }


        /// <summary>
        /// 멀티쓰레딩 테스트용
        /// </summary>
        private static Task<Workplace> CreateTaskTest(Workplace workplace, List<WorkBase> works)
        {
            return new Task<Workplace>(() =>
            {
                foreach (WorkBase work in works)
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

        private static void CopyBuffer(Workplace workplace, ref CopyBufferData buffer)
        {
            if (workplace.Width == 0 || workplace.Height == 0) return;

            if (workplace.SharedBufferInfo.PtrR_GRAY == IntPtr.Zero)
                return;

            int channelCount = workplace.SharedBufferInfo.PtrList.Count;

            if (buffer.Height * buffer.Width != workplace.Width * workplace.Height)
                buffer.Realloc(workplace.Width, workplace.Height, channelCount);

            for (int i = 0; i < channelCount; i++)
            {
                IntPtr ptr = workplace.SharedBufferInfo.PtrList[i];

                Tools.ParallelImageCopy(
                workplace.SharedBufferInfo.PtrList[i],
                workplace.SharedBufferInfo.Width,
                workplace.SharedBufferInfo.Height,
                new CRect
                (
                    workplace.PositionX,
                    workplace.PositionY + workplace.Height,
                    workplace.PositionX + workplace.Width,
                    workplace.PositionY),
                    buffer.BufferList[i]);
            }

            //Old

            //Tools.ParallelImageCopy(
            //    workplace.SharedBufferInfo.PtrR_GRAY,
            //    workplace.SharedBufferInfo.Width,
            //    workplace.SharedBufferInfo.Height,
            //    new CRect
            //    (
            //        workplace.PositionX,
            //        workplace.PositionY + workplace.Height,
            //        workplace.PositionX + workplace.Width,
            //        workplace.PositionY),
            //    buffer.BufferR_GRAY);


            //if (workplace.SharedBufferInfo.PtrG == IntPtr.Zero ||
            //   workplace.SharedBufferInfo.PtrB == IntPtr.Zero)
            //{
            //    return;
            //}

            //if (workplace.SharedBufferInfo.ByteCnt == 3)
            //{
            //    Tools.ParallelImageCopy(
            //        workplace.SharedBufferInfo.PtrG,
            //        workplace.SharedBufferInfo.Width,
            //        workplace.SharedBufferInfo.Height,
            //        new CRect
            //        (
            //            workplace.PositionX,
            //            workplace.PositionY + workplace.Height,
            //            workplace.PositionX + workplace.Width,
            //            workplace.PositionY),
            //        buffer.BufferG);

            //    Tools.ParallelImageCopy(
            //        workplace.SharedBufferInfo.PtrB,
            //        workplace.SharedBufferInfo.Width,
            //        workplace.SharedBufferInfo.Height,
            //        new CRect
            //        (
            //            workplace.PositionX,
            //            workplace.PositionY + workplace.Height,
            //            workplace.PositionX + workplace.Width,
            //            workplace.PositionY),
            //       buffer.BufferB);
            //}
        }
    }
}
