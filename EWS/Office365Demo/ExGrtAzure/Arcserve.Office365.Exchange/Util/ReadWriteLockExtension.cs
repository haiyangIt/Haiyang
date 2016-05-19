using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Util
{
    public static class ReadWriteLockExtension
    {
        private sealed class ReadLockToken : IDisposable
        {
            private ReaderWriterLockSlim _sync;
            public ReadLockToken(ReaderWriterLockSlim sync)
            {
                _sync = sync;
                sync.EnterReadLock();
            }
            public void Dispose()
            {
                if (_sync != null)
                {
                    _sync.ExitReadLock();
                    _sync = null;
                }
            }
        }
        private sealed class WriteLockToken : IDisposable
        {
            private ReaderWriterLockSlim _sync;
            public WriteLockToken(ReaderWriterLockSlim sync)
            {
                _sync = sync;
                sync.EnterWriteLock();
            }
            public void Dispose()
            {
                if (_sync != null)
                {
                    _sync.ExitWriteLock();
                    _sync = null;
                }
            }
        }

        public static IDisposable Read(this ReaderWriterLockSlim obj)
        {
            return new ReadLockToken(obj);
        }
        public static IDisposable Write(this ReaderWriterLockSlim obj)
        {
            return new WriteLockToken(obj);
        }

        public static IDisposable LockWhile<T>(this object obj, Action<T> action, T arg)
        {
            return new LockToken<T>(obj, action, arg);
        }

        public static IDisposable LockWhile(this object obj, Action action)
        {
            return new LockToken(obj, action);
        }

        public class LockToken : IDisposable
        {
            private object _obj;
            private Action _action;
            public LockToken(object obj, Action action)
            {
                _obj = obj;
                _action = action;
            }

            protected LockToken(object obj)
            {
                _obj = obj;
            }

            public void Dispose()
            {
                bool isBreak = false; ;
                do
                {
                    if (Monitor.TryEnter(_obj, 1000))
                    {
                        try
                        {
                            CallInternalInvoke();
                        }
                        finally
                        {
                            Monitor.Exit(_obj);
                        }
                        isBreak = true;
                    }
                } while (!isBreak);
            }

            protected virtual void CallInternalInvoke()
            {
                _action.Invoke();
            }
        }

        public sealed class LockToken<T> : LockToken
        {
            private object _obj;
            private Action<T> _actionT;
            private T _argument;
            public LockToken(object obj, Action<T> action, T arg) : base(obj)
            {
                _obj = obj;
                _actionT = action;
                _argument = arg;
            }

            protected override void CallInternalInvoke()
            {
                _actionT.Invoke(_argument);
            }
        }
    }
}
