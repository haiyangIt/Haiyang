using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using Arcserve.Office365.Exchange.DataProtect.Impl.Backup.Increment;
using Arcserve.Office365.Exchange.StorageAccess.MountSession;
using Arcserve.Office365.Exchange.EwsApi.Impl.Increment;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class SyncBackupTest
    {
        [TestMethod]
        public void TestSyncBackup()
        {
            SyncBackup backupFlow = new SyncBackup();
            TaskSyncContextBase taskSyncContextBase = new TaskSyncContextBase();
            backupFlow.InitTaskSyncContext(taskSyncContextBase);
            backupFlow.DataFromClient = new DataFromClient();
            backupFlow.DataFromClient.InitTaskSyncContext(taskSyncContextBase);
            var catalogFileName = string.Format("Catalog{0}.mdf", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            var catalogPath = string.Format(@"E:\10Test\01O365Test\Catalog1\{0}", catalogFileName);
            backupFlow.CatalogAccess = new CatalogAccess(catalogPath, "", @"E:\10Test\01O365Test\Catalog1\Data");
            backupFlow.CatalogAccess.InitTaskSyncContext(taskSyncContextBase);
            backupFlow.EwsServiceAdapter = new EwsServiceAdapter();
            backupFlow.EwsServiceAdapter.InitTaskSyncContext(taskSyncContextBase);
            backupFlow.DataConvert = new DataConvert();
            backupFlow.AdminInfo = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
            {
                OrganizationName = "arcserve",
                UserName = "devO365admin@arcservemail.onmicrosoft.com",
                UserPassword = "JackyMao1!"
            };
            backupFlow.BackupSync();
        }
    }
}
