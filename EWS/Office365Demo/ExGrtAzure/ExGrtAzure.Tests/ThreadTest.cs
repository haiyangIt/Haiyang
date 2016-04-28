using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

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
                    while (data.Count > 0)
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

        private async Task<int> DelayAndReturnAsync(int val)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Restart();
            await Task.Delay(TimeSpan.FromSeconds(val));
            stopWatch.Stop();
            Debug.WriteLine(string.Format("ThreadName:{0}:{1}", Thread.CurrentThread.ManagedThreadId, stopWatch.ElapsedMilliseconds));
            return val;
        }

        [TestMethod]
        public async Task TaskTest()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Restart();
            Task<int> taskA = DelayAndReturnAsync(4);
            Task<int> taskB = DelayAndReturnAsync(2);
            Task<int> taskC = DelayAndReturnAsync(3);
            var tasks = new Task<int>[] { taskA, taskB, taskC };
            foreach (var task in tasks)
            {
                var result = await task;
                Debug.WriteLine(result);
            }
            stopWatch.Stop();
            Debug.WriteLine("write");
            
            Debug.WriteLine(string.Format("Main ThreadName:{0}:{1}", Thread.CurrentThread.ManagedThreadId, stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();
            taskA = DelayAndReturnAsync(4);
            taskB = DelayAndReturnAsync(2);
            taskC = DelayAndReturnAsync(3);
            tasks = new Task<int>[] { taskA, taskB, taskC };
            var resultW = await Task.WhenAll(tasks);
            stopWatch.Stop();
            Debug.WriteLine(string.Format("result ,{0},{1},{2}", resultW[0], resultW[1], resultW[2]));
            Debug.WriteLine(string.Format("Main ThreadName:{0}:{1}", Thread.CurrentThread.ManagedThreadId, stopWatch.ElapsedMilliseconds));

            Debug.WriteLine("");
            stopWatch.Restart();
            Task<int>[] taskArray = new Task<int>[10];
            for(int i = 0; i < 10; i++)
            {
                taskArray[i] = DelayAndReturnAsync(i);
            }
            var resultaa = await Task.WhenAll(taskArray);
            stopWatch.Stop();
            Debug.WriteLine(string.Format("result ,{0},{1},{2}", resultaa[0], resultaa[1], resultaa[2]));
            Debug.WriteLine(string.Format("Main ThreadName:{0}:{1}", Thread.CurrentThread.ManagedThreadId, stopWatch.ElapsedMilliseconds));

        }

        [TestMethod]
        public async Task TaskLearn()
        {
            Stopwatch dTime = new Stopwatch();
            dTime.Start();
            List<Task> allTasks = new List<Task>();
            for(int i = 0;i < LoopCount; i++)
            {
                var task = CreateTask(i);
                task.Start();
                await task.ConfigureAwait(false);
                await task;
                allTasks.Add(task);
            }
            
            await Task.WhenAll(allTasks);
            dTime.Stop();
            Debug.WriteLine(string.Format("Time:{0}", dTime.ElapsedMilliseconds));
            OutputThread();
        }

        private void OutputThread()
        {
            Debug.WriteLine("Thread:");
            foreach (var value in dic.Keys)
            {
                Debug.Write(value);
                Debug.Write(" ");
            }
            Debug.WriteLine("");
            Debug.WriteLine(string.Format("ThreadName:{0}", Thread.CurrentThread.ManagedThreadId));
        }

        private Task CreateTask(int index)
        {
            return new Task(() =>
            {
                Thread.Sleep(2000);
                //Debug.WriteLine(string.Format("{1},ThreadName:{0}", Thread.CurrentThread.ManagedThreadId, index));
                dic[Thread.CurrentThread.ManagedThreadId] = true;
            });
        }

        ConcurrentDictionary<int, bool> dic = new ConcurrentDictionary<int, bool>();
        private const int LoopCount = 10;

        [TestMethod]
        public void TaskLearnParallel()
        {
            Stopwatch dTime = new Stopwatch();
            dTime.Start();
            var ints = new List<int>();

            for(int i = 0; i < LoopCount; i++)
            {
                ints.Add(i);
            }

            Parallel.ForEach<int>(ints, (index) =>
            {
                Thread.Sleep(2000);
                //Debug.WriteLine(string.Format("{1},ThreadName:{0}", Thread.CurrentThread.ManagedThreadId, index));
                dic[Thread.CurrentThread.ManagedThreadId] = true;
            });
            dTime.Stop();
            Debug.WriteLine(string.Format("Time:{0}", dTime.ElapsedMilliseconds));
            OutputThread();
        }

        [TestMethod]
        public async Task TaskLearnParallelAsync()
        {
            dic.Clear();
            Stopwatch dTime = new Stopwatch();

            dTime.Start();
            var ints = new List<int>();

            for (int i = 0; i < LoopCount; i++)
            {
                ints.Add(i);
            }

            var t = Task.Run(() =>
            {
                Parallel.ForEach<int>(ints, (index) =>
                {
                    Thread.Sleep(2000);
                    //Debug.WriteLine(string.Format("{1},ThreadName:{0}", Thread.CurrentThread.ManagedThreadId, index));
                    dic[Thread.CurrentThread.ManagedThreadId] = true;
                });
            }).ConfigureAwait(false);

            Debug.WriteLine("test1");
            await t;
           
            dTime.Stop();
            Debug.WriteLine(string.Format("Time:{0}", dTime.ElapsedMilliseconds));
            OutputThread();
        }

        private string[] GetMails()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestTplDataFlow()
        {
            
        }
    }
}
