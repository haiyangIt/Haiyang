using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Util
{
    public class Deque<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        public Deque()
        {
            _lists = new LinkedList<T>();
        }

        private LinkedList<T> _lists;
        public void Enqueue(T item)
        {
            using (_lists.LockWhile(() =>
            {
                _lists.AddFirst(item);
            }))
            { }
        }
        public T Dequeue()
        {
            T temp = default(T);
            using (_lists.LockWhile(() =>
            {
                if (_lists.Count == 0)
                {
                    temp = default(T);
                    return;
                }
                var result = _lists.Last;
                _lists.RemoveLast();
                temp = result.Value;
            }))
            { }
            return temp;
        }

        public void EnqueueLast(T item)
        {
            using (_lists.LockWhile(() =>
            {
                _lists.AddLast(item);
            }))
            { }
        }

        public int Count
        {
            get
            {
                return _lists.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return _lists;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            Enqueue(item);
        }

        public void Clear()
        {
            using (_lists.LockWhile(() =>
            {
                _lists.Clear();
            }))
            { }
        }

        public void CopyTo(Array array, int index)
        {
            using (_lists.LockWhile(() =>
            {
                T[] des = new T[Count - index];
                _lists.CopyTo(des, index);
                des.CopyTo(array, index);
            }))
            { }
        }

        public IEnumerator GetEnumerator()
        {
            IEnumerator result = null;
            using (_lists.LockWhile(() =>
            {
                result = _lists.GetEnumerator();
            }))
            { }
            return result;

        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerator<T> result = null;
            using (_lists.LockWhile(() =>
            {
                result = _lists.GetEnumerator();
            }))
            { }
            return result;
        }
    }
}
