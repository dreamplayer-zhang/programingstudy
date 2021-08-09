using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision.WorkManager3
{
    public class TaskManager<TResult>
    {
        public delegate void TaskCompletedHandler(TResult result);

        public event TaskCompletedHandler TaskCompleted;

        private FixedSizeList<Task<TResult>> tasks;

        private object lockObj = new object();

        #region [Properties]
        public int TaskCount
        {
            get => this.tasks.Count;
        }

        public int MaxTaskCount
        {
            get => this.tasks.FixedSize;
        }

        public bool IsAvailableTask
        {
            get => (TaskCount < this.MaxTaskCount);
        }

        #endregion

        public TaskManager(int size)
        {
            tasks = new FixedSizeList<Task<TResult>>(size);
            // 여기서 쓰레드 실행 종료 관리
        }

        public bool Invoke(Task<TResult> task)
        {
            try
            {
                bool result;
                //lock (lockObj) result = tasks.Add(task);
                result = tasks.Add(task);
                if (result)
                {
                    //IncreaseCount();
                    task.Start();
                    task.ContinueWith((arg) =>
                    {
                        Remove(task);
                    }, TaskContinuationOptions.ExecuteSynchronously);

                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
                TempLogger.Write("Worker", ex);
                return false;
            }
        }
        
        private void Remove(Task<TResult> task)
        {
            try
            {
                if (TaskCompleted != null)
                {
                    TaskCompleted(task.Result);
                }


                if (!this.tasks.Remove(task))
                {
                    //if(this.tasks.Count != 0)
                    //    throw new ArgumentException("삭제 시 false 반환되면 안됨");
                }
            }
            catch(Exception)
            {
                
            }
        }

        public void Clear()
        {
            if(this.tasks != null)
            {
                List<Task<TResult>> waitTask = new List<Task<TResult>>();

                lock (this.lockObj)
                {
                    foreach (Task<TResult> task in this.tasks.FixedList)
                    {
                        if (!task.IsCompleted || !task.IsCanceled)
                        {
                            waitTask.Add(task);
                        }
                    }
                }

                foreach (Task<TResult> task in waitTask)
                {
                    task.Wait(1000);
                }

                this.tasks.Clear();
            }
        }
    }
}
