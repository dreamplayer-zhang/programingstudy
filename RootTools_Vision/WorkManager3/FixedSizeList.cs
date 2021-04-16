using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager3
{
    public class FixedSizeList<T>
    {
        private int fixedSize;
        private List<T> list;

        #region [Properties]
        public int Count
        {
            get => this.list.Count;
        }

        public int FixedSize
        {
            get => this.fixedSize;
        }

        public List<T> FixedList
        {
            get => this.list;
        }

        #endregion

        private FixedSizeList() { }

        public FixedSizeList(int fixedSize)
        {
            this.list = new List<T>();
            this.fixedSize = fixedSize;

        }

        object lockObj = new object();
        public bool Add(T item)
        {
            lock (lockObj)
            {
                if (this.list.Count == fixedSize)
                    return false;

                list.Add(item);
                return true;
            }
        }

        public bool Remove(T item)
        {
            lock (lockObj)
            {
                bool rst = list.Remove(item);
                return rst;
            }
        }

        public int IndexOf(T item)
        {
            int index = -1;
            lock(lockObj)
            {
                index = this.list.IndexOf(item);
            }
            return index;
        }

        public void Clear()
        {
            lock (lockObj)
            {
                list.Clear();
            }
        }
    }
}
