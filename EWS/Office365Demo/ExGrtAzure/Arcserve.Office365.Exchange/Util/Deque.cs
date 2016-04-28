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
            lock (_lists)
            {
                _lists.AddFirst(item);
            }
        }
        public T Dequeue()
        {
            lock (_lists)
            {
                if (_lists.Count == 0)
                    return default(T);
                var result = _lists.Last;
                _lists.RemoveLast();
                return result.Value;
            }
        }

        public void EnqueueLast(T item)
        {
            lock (_lists)
            {
                _lists.AddLast(item);
            }
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
            lock (_lists)
            {
                _lists.Clear();
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (_lists)
            {
                T[] des = new T[Count - index];
                _lists.CopyTo(des, index);
                des.CopyTo(array, index);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _lists.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _lists.GetEnumerator();
        }
    }
}
