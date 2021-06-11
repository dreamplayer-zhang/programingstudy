using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools
{
    public class ObservableQueue<T> : INotifyCollectionChanged, IEnumerable<T>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private readonly Queue<T> queue = new Queue<T>();

        public void Enqueue(T item)
        {
            lock(queue)
            {
                queue.Enqueue(item);
            }
            if (CollectionChanged != null)
                CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, item));
        }

        public T Dequeue()
        {
            lock(queue)
            {
                var item = queue.Dequeue();
                return item;
            }
            //error남
            //if (CollectionChanged != null)
            //    CollectionChanged(this,
            //        new NotifyCollectionChangedEventArgs(
            //            NotifyCollectionChangedAction.Remove, item));
        }

        public T Peek()
        {
            lock(queue)
            {
                var item = queue.Peek();
                return item;
            }
        }

        public T[] ToArray()
        {
            lock(queue)
            {
                var array = queue.ToArray();
                return array;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        public void Clear()
        {
            queue.Clear();
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
