using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private int completeCount;
        public int CompleteCount
        {
            get=> this.completeCount;
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

        private object countLockObj = new object();
        public void IncreaseCount()
        {
            lock (countLockObj) this.completeCount++;
        }

        public void DecreaseCount()
        {
            lock (countLockObj) this.completeCount--;

            if (this.completeCount < 0)
            {
                MessageBox.Show("Bug");
            }
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
                    IncreaseCount();
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
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void Remove(Task<TResult> task)
        {
            if (TaskCompleted != null)
            {
                TaskCompleted(task.Result);
            }
                

            //lock(lockObj)
            {
                if (!this.tasks.Remove(task))
                {
                    //if(this.tasks.Count != 0)
                    //    throw new ArgumentException("삭제 시 false 반환되면 안됨");
                }
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
