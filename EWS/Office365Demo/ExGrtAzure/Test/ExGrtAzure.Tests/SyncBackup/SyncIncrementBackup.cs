using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.DataProtect.Impl;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Manager;
using Arcserve.Office365.Exchange.StorageAccess.MountSession;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.Backup;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Util.Setting;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExGrtAzure.Tests.SyncBackup
{
    [TestClass]
    public class SyncIncrementBackup
    {
        [TestMethod]
        public void TestIncrementBackup()
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            folder = Path.Combine(folder, "Increment");
            SyncBackupTest.DeleteDirectory(folder);

            //CloudConfig.Instance.IsTestForDemo = false;

            // init backup and full backup
            InitIncrementBackup();
            Thread.Sleep(2000);
            // full backup
            FullBackup(folder);
            Thread.Sleep(2000);
            // modify folder and items
            ModifyMailbox();
            Thread.Sleep(2000);
            // increment backup
            IncrementBackup(folder);

        }

        private const string FullBackupFolder = "FullCatalogFolder";
        private const string IncrementBackupFolder = "IncrementFolder";

        public void FullBackup(string workFolder)
        {
            try
            {
                var newCatalogFolder = Path.Combine(workFolder, FullBackupFolder);
                using (var catalogAccess = new CatalogAccess(newCatalogFolder, "", newCatalogFolder, "arcserve"))
                {
                    var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();
                    var dataClient = new DataFromClientFilterFolderForIncrement();
                    dataClient.InitTaskSyncContext(taskSyncContextBase);
                    catalogAccess.InitTaskSyncContext(taskSyncContextBase);

                    var ewsAdapter = EwsFactory.Instance.NewEwsAdapter();
                    ewsAdapter.InitTaskSyncContext(taskSyncContextBase);
                    var dataConvert = new DataConvert();
                    var adminInfo = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
                    {
                        OrganizationName = "arcserve",
                        UserName = "devO365admin@arcservemail.onmicrosoft.com",
                        UserPassword = "JackyMao1!"
                    };
                    using (var backupFlow = DataProtectFactory.Instance.NewBackupInstance(catalogAccess, ewsAdapter, dataClient, dataConvert, adminInfo))
                    {
                        backupFlow.InitTaskSyncContext(taskSyncContextBase);
                        backupFlow.BackupSync();
                    }
                }
            }
            finally
            {
                DisposeManager.DisposeInstance();
            }
        }

        public void IncrementBackup(string workFolder)
        {
            try
            {
                var newCatalogFolder = Path.Combine(workFolder, IncrementBackupFolder);
                var oldCatalogFileName = "arcserve".GetCatalogDatabaseFileName(CloudConfig.Instance.DbType.GetDatabaseType());
                var oldCatalogFolder = Path.Combine(workFolder, FullBackupFolder);
                var oldCatalogFilePath = Path.Combine(oldCatalogFolder, oldCatalogFileName);
                using (var catalogAccess = new CatalogAccess(newCatalogFolder, oldCatalogFolder, newCatalogFolder, "arcserve"))
                {
                    var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();
                    var dataClient = new DataFromClientFilterFolderForIncrement();
                    dataClient.InitTaskSyncContext(taskSyncContextBase);
                    catalogAccess.InitTaskSyncContext(taskSyncContextBase);

                    var ewsAdapter = EwsFactory.Instance.NewEwsAdapter();
                    ewsAdapter.InitTaskSyncContext(taskSyncContextBase);
                    var dataConvert = new DataConvert();
                    var adminInfo = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
                    {
                        OrganizationName = "arcserve",
                        UserName = "devO365admin@arcservemail.onmicrosoft.com",
                        UserPassword = "JackyMao1!"
                    };
                    using (var backupFlow = DataProtectFactory.Instance.NewBackupInstance(catalogAccess, ewsAdapter, dataClient, dataConvert, adminInfo))
                    {
                        backupFlow.InitTaskSyncContext(taskSyncContextBase);
                        backupFlow.BackupSync();
                    }
                }
            }
            finally
            {
                DisposeManager.DisposeInstance();
            }
        }

        internal const string AddFolderDisplayName = "00AddFolder";
        internal const string AddItemFolderDisplayName = "00AddItemFolder";
        internal const string DeleteItemFolderDisplayName = "00DeleteItemFolder";
        internal const string DeleteFolderName = "00DeleteFolder";
        internal const string ExistFolder = "00BDemoFolder";

        public void InitIncrementBackup()
        {
            //// 1. Delete AddFolder
            //// 2. Delete items in AddItem
            //// 3. Add items in DeleteItems
            //// 4. Add folder (DeleteFolder)
            //// 5. Change Read flag.

            var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();

            var ewsAdapter = EwsFactory.Instance.NewEwsAdpaterExtension();
            ewsAdapter.InitTaskSyncContext(taskSyncContextBase);

            ewsAdapter.GetExchangeService("haiyang.ling@arcserve.com", new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
            {
                OrganizationName = "arcserve",
                UserName = "devO365admin@arcservemail.onmicrosoft.com",
                UserPassword = "JackyMao1!"
            });

            var rootFolder = ewsAdapter.FolderBind(WellKnownFolderName.MsgFolderRoot);
            // 1. Delete AddFolder
            Folder folder = FindFolderByDisplayName(ewsAdapter, AddFolderDisplayName);
            if (folder != null)
                ewsAdapter.FolderDelete(folder, DeleteMode.HardDelete);

            // 2. Delete items in AddItem
            folder = FindFolderByDisplayName(ewsAdapter, AddItemFolderDisplayName);
            if (folder != null)
            {
                ewsAdapter.FolderEmpty(folder, DeleteMode.HardDelete, true);
            }
            else
            {
                ewsAdapter.FolderCreate(AddItemFolderDisplayName, FolderClass.Message.GetFolderClass(), rootFolder);
            }

            // 3. Add items in DeleteItems
            folder = FindFolderByDisplayName(ewsAdapter, DeleteItemFolderDisplayName);
            if (folder == null)
            {
                ewsAdapter.FolderCreate(DeleteItemFolderDisplayName, FolderClass.Message.GetFolderClass(), rootFolder);
                folder = FindFolderByDisplayName(ewsAdapter, DeleteItemFolderDisplayName);
            }
            if (folder != null)
            {
                ewsAdapter.FolderEmpty(folder, DeleteMode.HardDelete, true);
                using (MemoryStream stream = new MemoryStream(MsgFile.no_attach_simple_mail))
                    ewsAdapter.ImportItem(stream, folder.Id);
            }

            // 4. Add folder DeleteFolder
            folder = FindFolderByDisplayName(ewsAdapter, DeleteFolderName);
            if (folder == null)
            {
                ewsAdapter.FolderCreate(DeleteFolderName, FolderClass.Message.GetFolderClass(), rootFolder);

            }
            folder = FindFolderByDisplayName(ewsAdapter, DeleteFolderName);
            if (folder != null)
            {
                using (MemoryStream stream = new MemoryStream(MsgFile.test2))
                    ewsAdapter.ImportItem(stream, folder.Id);
                using (MemoryStream stream = new MemoryStream(MsgFile.test3))
                    ewsAdapter.ImportItem(stream, folder.Id);
            }
        }

        public void ModifyMailbox()
        {
            //// 1. Add AddFolder
            //// 2. Add items in AddItem
            //// 3. delete items in DeleteItems
            //// 4. delete folder (DeleteFolder)
            //// 5. Change Read flag.

            var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();

            var ewsAdapter = EwsFactory.Instance.NewEwsAdpaterExtension();
            ewsAdapter.InitTaskSyncContext(taskSyncContextBase);
            ewsAdapter.GetExchangeService("haiyang.ling@arcserve.com", new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
            {
                OrganizationName = "arcserve",
                UserName = "devO365admin@arcservemail.onmicrosoft.com",
                UserPassword = "JackyMao1!"
            });

            // 1. Add AddFolder
            var rootFolder = ewsAdapter.FolderBind(WellKnownFolderName.MsgFolderRoot);
            var folder = FindFolderByDisplayName(ewsAdapter, AddFolderDisplayName);
            if (folder == null)
                ewsAdapter.FolderCreate(AddFolderDisplayName, FolderClass.Message.GetFolderClass(), rootFolder);

            // 2. add items in AddItem
            folder = FindFolderByDisplayName(ewsAdapter, AddItemFolderDisplayName);
            if (folder != null)
            {
                using (MemoryStream stream = new MemoryStream(MsgFile.test))
                    ewsAdapter.ImportItem(stream, folder.Id);
            }
            else
            {
                throw new ArgumentException();
            }

            // 3. delete items in DeleteItems
            folder = FindFolderByDisplayName(ewsAdapter, DeleteItemFolderDisplayName);
            if (folder != null)
            {
                ewsAdapter.FolderEmpty(folder, DeleteMode.HardDelete, true);
            }
            else
            {
                throw new ArgumentException();
            }

            // 4. delete folder DeleteFolder
            folder = FindFolderByDisplayName(ewsAdapter, DeleteFolderName);
            if (folder != null)
                ewsAdapter.FolderDelete(folder, DeleteMode.HardDelete);
            else
            {
                throw new ArgumentException();
            }
        }

        private Folder FindFolderByDisplayName(IEwsServiceAdapterExtension<IJobProgress> ewsAdapter, string name)
        {
            var folderView = new FolderView(10);
            folderView.PropertySet = new PropertySet(FolderSchema.DisplayName, FolderSchema.Id);
            var searchFilter = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, name);
            var folderResult = ewsAdapter.FindFolders(WellKnownFolderName.MsgFolderRoot, searchFilter, new FolderView(10));
            if (folderResult.TotalCount == 1)
                return folderResult.Folders[0];
            else
                return null;
        }

        private IEnumerable<Item> GetAllItemsInFolder(IEwsServiceAdapterExtension<IJobProgress> ewsAdapter, string parentFolderName)
        {
            var folder = FindFolderByDisplayName(ewsAdapter, parentFolderName);
            if (folder == null)
            {
                return null;
            }

            const int pageSize = 100;
            int offset = 0;
            bool moreItems = true;
            List<Item> result = new List<Item>();
            ItemView oView = new ItemView(pageSize, offset, OffsetBasePoint.Beginning);
            oView.PropertySet = BasePropertySet.IdOnly;
            while (moreItems)
            {
                oView.Offset = offset;
                FindItemsResults<Item> findResult = ewsAdapter.FindItems(folder.Id, oView);

                foreach (var item in findResult.Items)
                {
                    result.Add(item);
                }

                if (!findResult.MoreAvailable)
                    moreItems = false;

                if (moreItems)
                    offset += pageSize;
            }
            return result;
        }

    }

    public class DataFromClientFilterFolderForIncrement : DataFromClient
    {
        private HashSet<string> inFolders = new HashSet<string>()
        {
            SyncIncrementBackup.AddFolderDisplayName,
            SyncIncrementBackup.AddItemFolderDisplayName,
            SyncIncrementBackup.DeleteFolderName,
            SyncIncrementBackup.DeleteItemFolderDisplayName,
            SyncIncrementBackup.ExistFolder
        };

        public override bool IsFolderInPlan(string uniqueFolderId)
        {
            return true;
        }

        public override bool IsItemValid(IItemDataSync item, IFolderDataSync parentFolder)
        {
            return true;
        }

        public override bool IsItemValid(string itemChangeId, IFolderDataSync parentFolder)
        {
            return true;
        }

        public override bool IsFolderInPlan(IFolderDataSync folderData)
        {
            if (inFolders.Contains(((IFolderDataBase)folderData).DisplayName))
            {
                return true;
            }
            return false;
        }
    }
}
