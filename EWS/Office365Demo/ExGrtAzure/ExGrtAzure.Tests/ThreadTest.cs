using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EwsFrame.Util;
using Microsoft.Exchange.WebServices.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EwsFrame;
using System.IO;
using System.Configuration;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class ThreadTest
    {
        [TestInitialize]
        public void TestInit()
        {
            string logFolder = ConfigurationManager.AppSettings["LogPath"];
            var logPath = Path.Combine(logFolder, string.Format("{0}Trace.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")));
            Trace.Listeners.Add(new TextWriterTraceListener(logPath));
        }

        [TestMethod]
        public void TestOperator()
        {
            try
            {
                List<int> items = new List<int>() { 0, 1, 2 };

                Parallel.ForEach(items, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, (item) =>
                 {
                     var b = new OperatorCtrlBaseImpl();
                     var timeOut = new TimeOutOperatorCtrl(b);
                     var retry = new RetryOperator(timeOut,
                         () =>
                         {
                             EwsRequestGate.Instance.Enter();
                         },
                         (e) =>
                         {
                             var type = e.GetType();
                             if (e is ServiceRequestException)
                             {
                                 EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(30,
                                     () =>
                                     {
                                         LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, "ServiceRequestException recovery.");
                                         Thread.Sleep(30);
                                     }, type)));
                             }
                             else if (e is OutOfMemoryException)
                             {
                                 EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(10, null, type)));
                             }
                             else if (e is TimeoutException)
                             {
                                 EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(10, null, type)));
                             }
                             else
                             {
                                 EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(10, null, type)));
                             }
                         });


                     retry.DoAction(() =>
                     {
                         try
                         {
                             int sleepTime = 1000;
                             switch (item)
                             {
                                 case 0:
                                     sleepTime = 1000;
                                     break;
                                 case 1:
                                     sleepTime = 5000;
                                     break;
                                 case 2:
                                     sleepTime = 10000;
                                     break;
                                 default:
                                     sleepTime = 20000;
                                     break;

                             }
                             LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, string.Format("Start {0} task, will sleep {1}s.", item, sleepTime));
                             Thread.Sleep(sleepTime);
                             throw new ApplicationException(string.Format("{0} task exception.", item));
                         }
                         finally
                         {
                             LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, string.Format("Exit {0} task", item));
                         }
                     });
                 });
            }
            catch (Exception ex)
            {
                LogFactory.LogInstance.WriteException(LogInterface.LogLevel.DEBUG, "test end.", ex, ex.Message);
            }
            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, "test end");

            Thread.Sleep(2000);
        }

        [TestCleanup]
        public void End()
        {
            LogFactory.LogInstance.Dispose();
        }
    }
}
