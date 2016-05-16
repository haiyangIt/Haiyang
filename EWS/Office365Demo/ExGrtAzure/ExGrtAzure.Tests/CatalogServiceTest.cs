using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EwsFrame;
using DataProtectInterface;
using System.IO;
using Microsoft.Exchange.WebServices.Data;
using EwsServiceInterface;
using System.Diagnostics;
using EwsDataInterface;
using Newtonsoft.Json;
using LogInterface;
using System.Threading;
using EwsFrame.Util;
using SqlDbImpl;
using System.Configuration;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class CatalogServiceTest
    {

        private static AutoResetEvent _wait = new AutoResetEvent(false);
        [ClassInitialize]
        public static void TestInit(TestContext context)
        {
            //TextWriterTraceListener myCreator = new TextWriterTraceListener(System.Console.Out);
            //Trace.Listeners.Add(myCreator);
            string logFolder = ConfigurationManager.AppSettings["LogPath"];
            var logPath = Path.Combine(logFolder, string.Format("{0}Trace.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")));
            Trace.Listeners.Add(new TextWriterTraceListener(logPath));

            IServiceContext serviceContext = ServiceContext.NewServiceContext("haiyang.ling@arcserve.com", "", "", "Arcserve", DataProtectInterface.TaskType.Catalog);
            serviceContext.CurrentMailbox = "haiyang.ling@arcserve.com";
            serviceContext.DataAccessObj.ResetAllStorage(serviceContext.CurrentMailbox);

            if (!FactoryBase.IsRunningOnAzureOrStorageInAzure())
            {

                using (CatalogDbContext dbContext = new CatalogDbContext(new SqlDbImpl.Model.OrganizationModel() { Name = "Arcserve" }))
                {
                    dbContext.Folders.RemoveRange(dbContext.Folders);
                    dbContext.ItemLocations.RemoveRange(dbContext.ItemLocations);
                    dbContext.Items.RemoveRange(dbContext.Items);
                    dbContext.Catalogs.RemoveRange(dbContext.Catalogs);
                    dbContext.Mailboxes.RemoveRange(dbContext.Mailboxes);

                    dbContext.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void TestInit()
        {

        }

        private static void Performance(object arg)
        {
            //PerformanceCounter cpuCounter = new PerformanceCounter();
            //PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            //Trace.TraceInformation(string.Format("ThreadCount\tworkingSet\tcpuprocessTime\tmemory available\t"));
            //while (true)
            //{
            //    Thread.Sleep(10000);
            //    //    var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            //    //    long workingSet = currentProcess.WorkingSet64;
            //    //    int threadCount = currentProcess.Threads.Count;

            //    //    Trace.TraceInformation("{0}\t{1}\t{2}\t{3}\t{4}", DateTime.Now.ToString("HHmmssfff"), threadCount, workingSet, cpuCounter.NextValue(), ramCounter.NextValue());
            //    Trace.Flush();
            //}
        }

        private static void ThreadBackup(object arg)
        {
            var argument = arg as CatalogArgs;
            try
            {
                using (argument.Service)
                    argument.Service.GenerateCatalog(argument.FilterItem);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, "Backup job failed.", e, e.Message);
            }
            finally
            {
                _wait.Set();
            }

        }

        class CatalogArgs
        {
            public ICatalogService Service;
            public IFilterItem FilterItem;
        }

        [TestMethod]
        public void TestGenerateCatalogPart()
        {
            TestCatalog(@"D:\21GitHub\Haiyang\EWS\Office365Demo\ExGrtAzure\ExGrtAzure.Tests\PartSelectItems.txt");
        }

        private void TestCatalog(string file)
        {
            var service = CatalogFactory.Instance.NewCatalogService("devO365admin@arcservemail.onmicrosoft.com", "JackyMao1!", "", "arcserve");
            var partSelect = "";
            using (StreamReader reader = new StreamReader(file))
            {
                partSelect = reader.ReadToEnd();
            }
            LoadedTreeItem selectedItem = JsonConvert.DeserializeObject<LoadedTreeItem>(partSelect);
            service.CatalogJobName = string.Format("Test Catalog job {0}", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            IFilterItem filterObj = CatalogFactory.Instance.NewFilterItemBySelectTree(selectedItem);
            var serviceId = Guid.NewGuid();
            JobProgressManager.Instance[serviceId] = (IDataProtectProgress)service;
            ThreadPool.QueueUserWorkItem(ThreadBackup, new CatalogArgs() { Service = service, FilterItem = filterObj });

           // ThreadPool.QueueUserWorkItem(Performance);
            _wait.WaitOne();
        }

        [TestMethod]
        public void TestGenerateCatalogAll()
        {
            TestCatalog(@"D:\21GitHub\Haiyang\EWS\Office365Demo\ExGrtAzure\ExGrtAzure.Tests\AllSelectItems.txt");
        }

        [ClassCleanup]
        public static void TestClearUp()
        {
            EwsRequestGate.Instance.Dispose();
            LogFactory.LogInstance.Dispose();
            LogFactory.EwsTraceLogInstance.Dispose();
            _wait.Dispose();
        }

    }
}
