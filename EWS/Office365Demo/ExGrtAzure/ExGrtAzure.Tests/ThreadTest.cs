using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class ThreadTest
    {
        int count = 2;
        AutoResetEvent quitEvent = new AutoResetEvent(false);

        AutoResetEvent prEvent = new AutoResetEvent(false);

        Queue<string> data = new Queue<string>();

        [TestMethod]
        public void TestAutoResetEvent()
        {
            Thread thread = new Thread(CreateThread);
            thread.Start();
            Thread threadEat = new Thread(EatThread);
            threadEat.Start();

            quitEvent.WaitOne();
        }

        private void CreateThread()
        {
            int i = 0;
            while (true)
            {
                Thread.Sleep(500);
                lock (data)
                {
                    data.Enqueue(i.ToString());
                    Debug.WriteLine(string.Format("Add {0}", i));
                }
                prEvent.Set();
                i++;
                if (i > 10)
                    break;
            }

            Quit();
        }

        private void EatThread()
        {
            while (true)
            {
                prEvent.WaitOne();
                List<string> d = new List<string>();
                string temp = string.Empty;
                lock (data)
                {
                    while(data.Count > 0)
                    {
                        temp = data.Dequeue();
                        d.Add(temp);
                        Debug.WriteLine(string.Format("Eat {0}", temp));
                    }
                }

                if (temp == "10")
                    break;

                Thread.Sleep(100);

            }
            Quit();
        }

        private void Quit()
        {
            Interlocked.Decrement(ref count);
            if (Interlocked.CompareExchange(ref count, -11, 0) == 0)
            {
                quitEvent.Set();
            }
        }
    }
}
