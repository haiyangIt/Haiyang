using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.EwsApi.Impl.Increment;
using Microsoft.Exchange.WebServices.Data;
using System.Threading;
using System.Diagnostics;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Collections.Generic;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.DataProtect.Impl.Backup;
using System.IO;
using System.Management;
using System.Security.Permissions;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.DataProtect.Impl;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class EwsServiceApiTest
    {
        private IEwsServiceAdapter<IJobProgress> CreatAdapter()
        {
            ITaskSyncContext<IJobProgress> taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();
            IEwsServiceAdapter<IJobProgress> adpapter = EwsFactory.Instance.NewEwsAdapter();
            adpapter.InitTaskSyncContext(taskSyncContextBase);
            return adpapter;
        }

        [TestMethod]
        public void TestSyncItems()
        {
            CloudConfig.Instance.MaxItemChangesReturn = 50;
            TestSyncItemsTime();
            CloudConfig.Instance.MaxItemChangesReturn = 100;
            TestSyncItemsTime();
            CloudConfig.Instance.MaxItemChangesReturn = 150;
            TestSyncItemsTime();
            CloudConfig.Instance.MaxItemChangesReturn = 200;
            TestSyncItemsTime();
            CloudConfig.Instance.MaxItemChangesReturn = 250;
            TestSyncItemsTime();
        }

        private void TestSyncItemsTime()
        {
            IEwsServiceAdapter<IJobProgress> adpapter = CreatAdapter();
            adpapter.GetExchangeService("haiyang.ling@arcserve.com", new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
            {
                OrganizationName = "arcserve",
                UserName = "devO365admin@arcservemail.onmicrosoft.com",
                UserPassword = "JackyMao1!"
            });

            Arcserve.Office365.Exchange.Util.PerformanceCounter counter = Arcserve.Office365.Exchange.Util.PerformanceCounter.Start();
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(3000);
                ChangeCollection<ItemChange> folderChanges = null;
                var lastSyncStatus = string.Empty;
                counter.Suspend(false);
                do
                {
                    Thread.Sleep(1500);
                    counter.Resume();
                    folderChanges = adpapter.SyncItems("AAMkAGYxYzc0MTAyLTI3MjAtNDA5Zi04ZDY2LTlmODU5NmJkZDlhZgAuAAAAAADKmQFKsxwfSKwEXH3khGtpAQA1BzsargwHRq9aRKbs1Mp0AAAx45vvAAA=", lastSyncStatus);
                    counter.Suspend();
                    lastSyncStatus = folderChanges.SyncState;
                } while (folderChanges.MoreChangesAvailable);
                var seconds = counter.EndBySecond();
                counter.DoForEachTimeSpan((t) =>
                {
                    Debug.WriteLine("   cost {0} time", t.TotalMilliseconds);
                });
                Debug.WriteLine("{0} time cost {1}s", i, seconds);
                counter.Reset();
            }
        }

        [TestMethod]
        public void TestSyncFolder()
        {
            try {
                IEwsServiceAdapter<IJobProgress> adpapter = CreatAdapter();

                adpapter.GetExchangeService("haiyang.ling@arcserve.com", new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
                {
                    OrganizationName = "arcserve",
                    UserName = "devO365admin@arcservemail.onmicrosoft.com",
                    UserPassword = "JackyMao1!"
                });

                Arcserve.Office365.Exchange.Util.PerformanceCounter counter = Arcserve.Office365.Exchange.Util.PerformanceCounter.Start();
                var result = adpapter.SyncFolderHierarchy(string.Empty);
                var seconds = counter.EndBySecond();

                List<IFolderDataSync> data = new List<IFolderDataSync>(result.Count);
                IDataConvert dataConvert = new DataConvert();
                var mailboxInfo = new MailboxDataSyncBase()
                {
                    MailAddress = "haiyang.ling@arcserve.com",
                    Id = "0"

                };

                Arcserve.Office365.Exchange.Util.PerformanceCounter counter1 = Arcserve.Office365.Exchange.Util.PerformanceCounter.Start();
                FolderTree folderTree = new FolderTree();
                foreach (var folder in result)
                {
                    counter1.Resume();
                    adpapter.LoadFolderProperties(folder.Folder);
                    counter1.Suspend();
                    var folderSync = dataConvert.Convert(folder.Folder, mailboxInfo);
                    folderTree.AddNode(folderSync);
                }
                folderTree.AddComplete();

                counter1.DoForEachTimeSpan((t) =>
                {
                    Debug.WriteLine("    Load Folder property cost {0} time", t.TotalMilliseconds);
                });

                Debug.WriteLine("    {0} time cost {1}s", result.Count, counter1.EndBySecond());

                Debug.WriteLine(" {0} time cost {1}s", result.Count, seconds);
            }
            catch (Exception e)
            {
                Debug.WriteLine(" {0} time cost {1}", e.Message, e.StackTrace);
            }
        }
        
        [TestMethod]
        public void TestGetMailbox()
        {
            IEwsServiceAdapter<IJobProgress> adpapter = CreatAdapter();
            var org = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
            {
                OrganizationName = "arcserve",
                UserName = "devO365admin@arcservemail.onmicrosoft.com",
                UserPassword = "JackyMao1!"
            };
            var mailboxes = adpapter.GetAllMailboxes(org.UserName, org.UserPassword, new List<string>(2) { "haiyang.ling@arcserve.com", "shiqiang.li@arcserve.com" });
        }

        [TestMethod]
        public void TestBindFolder()
        {
            var adpapter = CreatAdapter();
            var org = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
            {
                OrganizationName = "arcserve",
                UserName = "devO365admin@arcservemail.onmicrosoft.com",
                UserPassword = "JackyMao1!"
            };
            adpapter.GetExchangeService("haiyang.ling@arcserve.com", org);
            var result = adpapter.SyncFolderHierarchy(string.Empty);
            foreach(var folder in result)
            {
                adpapter.LoadFolderProperties(folder.Folder);
                
                if (folder.Folder.Id.FolderName.HasValue)
                {
                    Debug.WriteLine(folder.FolderId.FolderName.Value);
                }
            }
        }
    }
}
