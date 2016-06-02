using Arcserve.Office365.Exchange.EwsApi;
using Arcserve.Office365.Exchange.EwsApi.Impl.Common;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.Manager.Data;
using Arcserve.Office365.Exchange.Manager.IF;
using Arcserve.Office365.Exchange.Manager.Impl;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Thread.CurrentThread.Name = "MainGUI";
            InitializeComponent();
            //workThread = new Thread(workfunc);
            //workThread.Start();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            Thread perThread = new Thread(PerformanceVisitor);
            perThread.Start();
        }

        Thread workThread ;
        AutoResetEvent startEvent = new AutoResetEvent(false);
        AutoResetEvent endEvent = new AutoResetEvent(false);

        private void workfunc()
        {
            startEvent.WaitOne();
            //LogFactory.LogInstance.RegisterLogStream(new TestboxStreamProvider(this));
            JobFactoryServer.OnStart();
            JobFactoryServer.Instance.ProgressManager.NewProgressEvent += ProgressManager_NewProgressEvent;

            endEvent.WaitOne();
            JobFactoryServer.OnStop();
        }

        private List<TestJob> _allJobs = new List<TestJob>();

        private void ProgressManager_NewProgressEvent(object sender, ProgressArgs e)
        {
            UpdateListbox();
            TestProgressInfo info = e.ProgressInfo as TestProgressInfo;
            AppendToTextbox1(string.Format("{0}\t{1}\t{2}\t{3}/{4}\r\n", info.Time, info.Job.JobId, info.Job.Status, info.CurrentStep, info.TotalStep));
        }


        bool EnsureMainGuiThread()
        {
            if (Thread.CurrentThread.Name != "MainGUI")
            {
                return false;
            }
            return true;
        }

        public void UpdateListbox()
        {
            if (EnsureMainGuiThread())
            {
                UpdateList(_allJobs);
                return;
            }
            
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action(UpdateListbox));
                return;
            }

            UpdateList(_allJobs);
        }

        private void UpdateList(List<TestJob> list)
        {
            JobList.Items.Clear();
            foreach(var item in list)
            {
                JobList.Items.Add(item);
            }
        }

        public void AddListbox(TestJob job)
        {
            _allJobs.Add(job);
            UpdateListbox();
        }

        public void AppendToTextbox1(string text)
        {
            if (EnsureMainGuiThread())
            {
                textBox1.AppendText(text);
                return;
            }
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendToTextbox1), new object[] { text });
                return;
            }
            
            textBox1.AppendText(text);
        }
        

        private void BtnAddJob_Click(object sender, EventArgs e)
        {
            TestJob job = new TestJob();
            
            JobFactoryServer.Instance.JobManager.AddJob(job);
            AddListbox(job);
            job.JobStatusChangedEvent += Job_JobStatusChangedEvent;
        }

        private void Job_JobStatusChangedEvent(object sender, JobStatusChangedEventArgs e)
        {
            UpdateListbox();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            startEvent.Set();
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            endEvent.Set();
        }

        private void BtnCancelJob_Click(object sender, EventArgs e)
        {
            IArcJob job = (TestJob)JobList.SelectedItem;
            job = JobFactoryServer.Instance.JobManager.GetJob(job.JobId);
            job.CancelJob();
        }

        //public class TestboxStreamProvider : ILogStreamProvider
        //{
        //    private Form1 _form;
        //    private Guid _streamId = Guid.NewGuid();
        //    public TestboxStreamProvider(Form1 form)
        //    {
        //        _form = form;
        //    }

        //    public Guid StreamId
        //    {
        //        get
        //        {
        //            return _streamId;
        //        }
        //    }

        //    public object SyncObj
        //    {
        //        get
        //        {
        //            return _form;
        //        }
        //    }

        //    public string GetTotalLog(DateTime date)
        //    {
        //        return _form.textBox1.Text;
        //    }

        //    public void Write(string information)
        //    {
        //        Debug.Write(information);
        //        _form.AppendToTextbox1(information + "\r\n");
        //    }

        //    public void WriteLine(string information)
        //    {
        //        Debug.WriteLine(information);
        //        _form.AppendToTextbox1(information + "\r\n");
        //    }
        //}

        private void AddLine_Click(object sender, EventArgs e)
        {
            AppendToTextbox1("\r\n");
        }

        private void Init_Click(object sender, EventArgs e)
        {
            //currentService = GetExchangeService();
            //WriteAllItemToFile();
        }

        private async void TestAsync_Click(object sender, EventArgs e)
        {
            _startEvent.Set();
            Thread thread = new Thread(TestAsyncThread);
            thread.IsBackground = true;
            thread.Start();

            await System.Threading.Tasks.Task.Run(() =>
            {
                _endEvent.Wait();
                Thread.Sleep(200);
            });
            _endEvent.Reset();
            this.textBox1.AppendText("Finished");
        }
        private async void TestParallel_Click(object sender, EventArgs e)
        {
            _startEvent.Set();
            Thread thread = new Thread(TestParallelThread);
            thread.IsBackground = true;
            thread.Start();

            await System.Threading.Tasks.Task.Run(() =>
            {
                _endEvent.Wait();
                Thread.Sleep(1000);
            });
            _endEvent.Reset();
            this.textBox1.AppendText("Finished\r\n");
        }

        private ExchangeService currentService;

        //private ExchangeService GetExchangeService()
        //{
        //    string mailbox = "haiyang.ling@arcserve.com";
        //    string userName = "haiyang.ling@arcserve.com";
        //    string password = "LhyWrf4$4";
        //    EwsServiceArgument argument = new EwsServiceArgument();
        //    argument.UseDefaultCredentials = false;
        //    argument.ServiceCredential = new System.Net.NetworkCredential(userName, password);
        //    argument.RequestedExchangeVersion = ExchangeVersion.Exchange2013_SP1;
        //    argument.EwsUrl = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");
        //    var ewsService = EwsProxyFactory.CreateExchangeService(argument, mailbox);
        //    return ewsService;
        //}
        //public void WriteAllItemToFile()
        //{
        //    if (File.Exists("ItemIds.txt"))
        //        return;

        //    //currentService = GetExchangeService();
        //    var ewsService = currentService;
        //    SearchFilter filter = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, "05Study");
        //    var result = currentService.FindFolders(WellKnownFolderName.Inbox, filter, new FolderView(10));

        //    var folder = result.FirstOrDefault();

        //    //currentService.SyncFolderItems(folder.Id, PropertySet.IdOnly, null, 10, SyncFolderItemsScope.NormalItems, string.Empty);

        //    List<string> itemIds = new List<string>(20);
        //    bool hasMoreChanges = false;
        //    var lastSync = string.Empty;
        //    do
        //    {
        //        var itemChanges = currentService.SyncFolderItems(folder.Id, PropertySet.IdOnly, null, 10, SyncFolderItemsScope.NormalItems, lastSync); 
        //        hasMoreChanges = itemChanges.MoreChangesAvailable;

        //        if (itemChanges.Count > 0)
        //        {
        //            var changeItems = from i in itemChanges select i.Item;

        //            foreach (var change in itemChanges)
        //            {
        //                itemIds.Add(change.ItemId.UniqueId);
        //            }
        //        }
        //        lastSync = itemChanges.SyncState;
        //    } while (hasMoreChanges);

        //    using (StreamWriter writer = new StreamWriter("ItemIds.txt"))
        //    {
        //        writer.Write(JsonConvert.SerializeObject(itemIds));
        //    }


        //}

        private List<string> ReadAllItemIds()
        {
            using (StreamReader reader = new StreamReader("ItemIds.txt"))
            {
                string ids = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<string>>(ids);
            }
        }

        private void TestAsyncThread()
        {
            try {
                Thread.Sleep(500);
                DateTime startTime = DateTime.Now;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var result = TestExchangeAsync();
                result.Wait();
                stopWatch.Stop();

                DateTime endTime = DateTime.Now;
                using (StreamWriter writer = new StreamWriter("Perf.txt", true))
                {
                    writer.WriteLine(string.Format("Start Time :{1}, End Time:{2}. Async test:[{0}].", stopWatch.ElapsedMilliseconds, startTime.ToString("HHmmssff"), endTime.ToString("HHmmssfff")));
                }
            }
            catch(Exception ex)
            {
                using (StreamWriter writer = new StreamWriter("Perf.txt", true))
                {
                    writer.WriteLine(string.Format("Async test ex:[{0}].", ex.Message));
                }
            }
            finally
            {
                _endEvent.Set();
            }
        }

        private void TestParallelThread()
        {
            try {
                Thread.Sleep(500);
                DateTime startTime = DateTime.Now;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                TestExchangeParallel();
                stopWatch.Stop();
                DateTime endTime = DateTime.Now;

                using (StreamWriter writer = new StreamWriter("Perf.txt", true))
                {
                    writer.WriteLine(string.Format("Start Time :{1}, End Time:{2}. TestParallel test:[{0}].", stopWatch.ElapsedMilliseconds, startTime.ToString("HHmmssff"), endTime.ToString("HHmmssfff") ));
                }
            }
            catch(Exception ex)
            {
                using (StreamWriter writer = new StreamWriter("Perf.txt", true))
                {
                    writer.WriteLine(string.Format("TestParallel test ex:[{0}].", ex.Message));
                }
            }
            finally
            {
                _endEvent.Set();
            }
        }

        private async System.Threading.Tasks.Task TestExchangeAsync()
        {
            var ids = ReadAllItemIds();
            List<System.Threading.Tasks.Task<Item>> tasks = new List<System.Threading.Tasks.Task<Item>>(ids.Count);
            foreach (var id in ids)
            {
                tasks.Add(GetItem(id));
            }

            var results = await System.Threading.Tasks.Task.WhenAll(tasks);
            

            var itemArray = Partition(results);

            List<System.Threading.Tasks.Task> taskArray = new List<System.Threading.Tasks.Task>(itemArray.Count);
            foreach (var id in itemArray)
            {
                taskArray.Add(GetItemDetail(id));
            }

            await System.Threading.Tasks.Task.WhenAll(taskArray);
        }

        private async System.Threading.Tasks.Task<Item> GetItem(string itemId)
        {
            var result = await System.Threading.Tasks.Task.Run<Item>(() =>
            {
                return Item.Bind(currentService, new ItemId(itemId), PropertySet.IdOnly);
            });
            return result;
        }

        private async System.Threading.Tasks.Task GetItemDetail(List<Item> items)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                currentService.LoadPropertiesForItems(items, PropertySet.FirstClassProperties);
            });
        }

        private void TestExchangeParallel()
        {
            var ids = ReadAllItemIds();
            var results = new ConcurrentBag<Item>();
            System.Threading.Tasks.Parallel.ForEach(ids, (id, state) =>
            {
                results.Add(Item.Bind(currentService, new ItemId(id), PropertySet.IdOnly));
            });

            List<List<Item>> itemArray = new List<List<Item>>();

            var splits = Partition(results);

            System.Threading.Tasks.Parallel.ForEach(splits, (items, state) => {
                currentService.LoadPropertiesForItems(items, PropertySet.FirstClassProperties);
            });
        }

        private List<List<Item>> Partition(IEnumerable<Item> items)
        {
            int index = 0;

            List<List<Item>> result = new List<List<Item>>();
            List<Item> temp = null;
            bool isLastAdd = false;
            foreach (var item in items)
            {
                if(index == 0)
                {
                    temp = new List<Item>(3);
                }

                temp.Add(item);
                if (index == 2)
                {
                    index = 0;
                    result.Add(temp);
                    isLastAdd = true;
                }
                else
                {
                    index++;
                    isLastAdd = false;
                }
            }
            if (!isLastAdd)
                result.Add(temp);

            return result;
        }


        private AutoResetEvent _startEvent = new AutoResetEvent(false);
        private ManualResetEventSlim _endEvent = new ManualResetEventSlim(false);
        PerformanceCounter cpuCounter = new PerformanceCounter();
        PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");



        private void PerformanceVisitor()
        {
            while (true)
            {
                _startEvent.WaitOne();
                StreamWriter writer = null;
                try
                {
                    writer = new StreamWriter("PerfDetail.txt", true);
                    writer.WriteLine(string.Format("ThreadCount\tworkingSet\tcpuprocessTime\tmemory available\t"));
                    writer.WriteLine("");
                    writer.WriteLine("--------------------");
                    writer.WriteLine("");
                    while (!_endEvent.Wait(200))
                    {
                        var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                        long workingSet = currentProcess.WorkingSet64;
                        int threadCount = currentProcess.Threads.Count;

                        writer.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                            DateTime.Now.ToString("HHmmssfff"), threadCount, workingSet, cpuCounter.NextValue(), ramCounter.NextValue()));
                    }
                    writer.WriteLine("");
                    writer.WriteLine("--------------------");
                    writer.WriteLine("");
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                        writer.Dispose();
                    }
                }
            }
        }

        private ManualResetEventSlim writeEvent = new ManualResetEventSlim(false);

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _startEvent.Dispose();
            _endEvent.Dispose();
        }
    }

}
