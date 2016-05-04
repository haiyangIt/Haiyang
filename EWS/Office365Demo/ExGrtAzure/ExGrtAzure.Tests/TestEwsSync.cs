using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EwsService.Common;
using EwsServiceInterface;
using Microsoft.Exchange.WebServices.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class TestEwsSync
    {
        [TestMethod]
        public void TestSyncFolder()
        {
            TestSync(string.Empty);
        }

        private void TestSync(string lastSyncState)
        {
            EwsServiceArgument argument = new EwsServiceArgument();
            argument.SetXAnchorMailbox = true;
            argument.ServiceCredential = new System.Net.NetworkCredential("devO365admin@arcservemail.onmicrosoft.com", "arcserve1!");
            argument.XAnchorMailbox = "haiyang.ling@arcserve.com";
            argument.ServiceEmailAddress = "haiyang.ling@arcserve.com";
            argument.UserToImpersonate = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, "haiyang.ling@arcserve.com");

            var service = EwsProxyFactory.CreateExchangeService(argument, "haiyang.ling@arcserve.com");

            var fcc = service.SyncFolderHierarchy(new FolderId(WellKnownFolderName.MsgFolderRoot), PropertySet.IdOnly, lastSyncState);



            if (fcc.Count == 0)
            {
                Debug.WriteLine("There are no folders to synchronize.");
            }

            // Otherwise, write all the changes included in the response 
            // to the console. 
            // For the initial synchronization, all the changes will be of type
            // ChangeType.Create.
            else
            {
                PropertySet set = new PropertySet(FolderSchema.Id, FolderSchema.DisplayName, FolderSchema.ParentFolderId, FolderSchema.FolderClass);
                var moreInfoSet = new PropertySet(FolderSchema.ChildFolderCount, FolderSchema.TotalCount, FolderSchema.UnreadCount, FolderSchema.WellKnownFolderName);
                foreach (FolderChange fc in fcc)
                {
                    Debug.Write("ChangeType: " + fc.ChangeType.ToString());
                    Debug.Write("\tFolderId: " + fc.FolderId);


                    Folder folder = Folder.Bind(service, fc.FolderId, set);



                    foreach (var setItem in set)
                    {
                        Debug.Write("\t");
                        Debug.Write(setItem.ToString());
                        Debug.Write(":");
                        object value = null;
                        if (folder.TryGetProperty(setItem, out value) && value != null)
                            Debug.Write(value.ToString());
                        else
                            Debug.Write("NULL");
                    }

                    Debug.WriteLine("===========");
                }

            }

            // Save the sync state for use in future SyncFolderItems requests.
            // The sync state is used by the server to determine what changes to report
            // to the client.
            string fSyncState = fcc.SyncState;
            Debug.WriteLine("State:" + fSyncState);
        }

        [TestMethod]
        public void TestSyncFolderFromLastSync()
        {
            TestSync("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");
        }

        public void TestSyncFolderFlow()
        {
            // 1. Create Backup Plan
            // 1.1 Backup time interval, backup mailbox and folders, backup account setting,

            // 2. Backup Strategy 
            // 2.1 For each mail, we need 
        }

        [TestMethod]
        public void TestInterlock()
        {
            _otherObj = new TestEwsSync();
            Parallel.For(0, 100, (i) =>
            {
                TestEwsSync.Instance.DoSomething();
            });

            Debug.WriteLine(counter);
        }

        public void DoSomething()
        {
            Thread.Sleep(20);
        }

        private static TestEwsSync _otherObj = null;
        private static TestEwsSync _instance = null;
        private static TestEwsSync Instance
        {
            get
            {
                //if (_instance == null)
                //{
                //    lock (_otherObj)
                //    {
                //        if (_instance == null)
                //            _instance = CreateInstance();
                //    }
                //}

                //return _instance;

                if (_instance == null)
                    Interlocked.CompareExchange(ref _instance, CreateInstance(), null);

                //var value = Interlocked.CompareExchange(ref _instance, _otherObj, null);
                //if (value == null)
                //    Interlocked.Increment(ref counter);
                return _instance;
            }
        }

        private static int counter = 1;
        private static TestEwsSync CreateInstance()
        {
            Interlocked.Increment(ref counter);
            Thread.Sleep(10);
            return new TestEwsSync();
        }
    }
}
