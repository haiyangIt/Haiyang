using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF;
using System.IO;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Linq;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class EntityFrameWorkTest
    {

        private string MdfDbFolder
        {
            get
            {
                var result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Db");
                if (!Directory.Exists(result))
                {
                    Directory.CreateDirectory(result);
                }
                return result;
            }
        }

        private string MdfOldFolder
        {
            get
            {
                return string.Format("{0}\\Db\\Old", AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        private string MdfNewFolder
        {
            get
            {
                return string.Format("{0}\\Db\\New", AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        private string MdfOldFile
        {
            get
            {
                return string.Format("{0}\\Db\\Old\\CatalogOld.mdf", AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        private string LogOldFile
        {
            get
            {
                return string.Format("{0}\\Db\\Old\\CatalogOld_log.ldf", AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        private string MdfOldInitCatalog
        {
            get
            {
                return "OldCatalog";
            }
        }

        private string MdfNewFile
        {
            get
            {
                return string.Format("{0}\\Db\\New\\CatalogNew.mdf", AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        private string LogNewFile
        {
            get
            {
                return string.Format("{0}\\Db\\New\\CatalogNew_log.ldf", AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        private string MdfNewInitCatalog
        {
            get
            {
                return "NewCatalog";
            }
        }

        private IEnumerable<string> FindMdfFile(string folder, bool isDelete)
        {
            var files = Directory.EnumerateFiles(folder, "*.*");
            if (isDelete)
            {
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                return new string[0];
            }
            else
            {
                return files;
            }
        }

        [TestMethod]
        public void CreateMdfWithInitCatalog()
        {
            System.Data.SqlClient.SqlConnection.ClearAllPools();
            var catalogWithInitCatalog = Path.Combine(MdfDbFolder, "CatalogWithInitCatalog");
            var sourMdf = Path.Combine(catalogWithInitCatalog, "Sourc");
            var desMdf = Path.Combine(catalogWithInitCatalog, "Des");
            if(!Directory.Exists(sourMdf))
            {
                Directory.CreateDirectory(sourMdf);
            }
            else
            {
                Directory.Delete(sourMdf, true);
                Thread.Sleep(2000);
                Directory.CreateDirectory(sourMdf);
            }
            if (!Directory.Exists(desMdf))
            {
                Directory.CreateDirectory(desMdf);
            }
            else
            {
                Directory.Delete(desMdf, true);
                Thread.Sleep(2000);
                Directory.CreateDirectory(desMdf);
            }

            var mdfFileName = "Arcserve.mdf";
            var logFileName = "Arcserve_log.ldf";
            var catalogName = "Arcserve";
            var catalogDesName = "Arcserve1";
            var sourMdfFile = Path.Combine(sourMdf, mdfFileName);
            var desMdfFile = Path.Combine(desMdf, mdfFileName);
            var sourLogFile = Path.Combine(sourMdf, logFileName);
            var defLogFile = Path.Combine(desMdf, logFileName);

            

            CreateMdfFile(sourMdfFile, true, catalogName);
            System.Data.SqlClient.SqlConnection.ClearAllPools();
            File.Copy(sourMdfFile, desMdfFile);
            File.Copy(sourLogFile, defLogFile);

            CreateMdfFile(desMdfFile, true, catalogDesName);
            CreateMdfFile(sourMdfFile, true, catalogName);
            AsscessMdfFile(sourMdfFile, true, catalogName);
            AsscessMdfFile(desMdfFile, true, catalogDesName);
        }

        [TestMethod]
        public void CreateMdfWithOutInitCatalog()
        {
            System.Data.SqlClient.SqlConnection.ClearAllPools();
            var catalogWithInitCatalog = Path.Combine(MdfDbFolder, "CatalogWithOutInitCatalog");
            var sourMdf = Path.Combine(catalogWithInitCatalog, "Sourc");
            var desMdf = Path.Combine(catalogWithInitCatalog, "Des");
            if (!Directory.Exists(sourMdf))
            {
                Directory.CreateDirectory(sourMdf);
            }
            else
            {
                Directory.Delete(sourMdf, true);
                Thread.Sleep(2000);
                Directory.CreateDirectory(sourMdf);
            }
            if (!Directory.Exists(desMdf))
            {
                Directory.CreateDirectory(desMdf);
            }
            else
            {
                Directory.Delete(desMdf, true);
                Thread.Sleep(2000);
                Directory.CreateDirectory(desMdf);
            }

            var mdfFileName = "Arcserve.mdf";
            var logFileName = "Arcserve_log.ldf";
            var catalogName = "Arcserve";
            var sourMdfFile = Path.Combine(sourMdf, mdfFileName);
            var desMdfFile = Path.Combine(desMdf, mdfFileName);
            var sourLogFile = Path.Combine(sourMdf, logFileName);
            var defLogFile = Path.Combine(desMdf, logFileName);

            CreateMdfFile(sourMdfFile, false, catalogName);
            //System.Data.SqlClient.SqlConnection.ClearAllPools();
            File.Copy(sourMdfFile, desMdfFile);
            File.Copy(sourLogFile, defLogFile);

            CreateMdfFile(desMdfFile, false, catalogName);
            AsscessMdfFile(sourMdfFile, false, catalogName);
            AsscessMdfFile(desMdfFile, false, catalogName);

            DeleteMdfFile(desMdfFile, catalogName);
            AsscessMdfFile(desMdfFile, false, catalogName);
        }

       

        private void CreateMdfFile(string fileName, bool isInitCatalog, string catalogName)
        {
            using (CatalogSyncDbContext context = new CatalogSyncDbContext(fileName))
            {
                MailboxSyncModel model = new MailboxSyncModel()
                {
                    Id = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    DisplayName = "test",
                    MailAddress = string.Format("test{0}@arcserve.com", DateTime.Now.ToString("yyyyMMddHHmmss")),
                    SyncStatus = string.Empty,
                    Name = "test"

                };
                context.Mailboxes.Add(model);
                context.SaveChanges();
                SqlConnection.ClearPool(context.Database.Connection as SqlConnection);
            }
            
        }

        private void AsscessMdfFile(string fileName, bool isInitCatalog, string catalogName)
        {
            using (CatalogSyncDbContext context = new CatalogSyncDbContext(fileName))
            {
                var mailbox = context.Mailboxes;
                foreach (var m in mailbox)
                {
                    Debug.WriteLine(m.MailAddress);
                }
                SqlConnection.ClearPool(context.Database.Connection as SqlConnection);
            }
        }

        private void DeleteMdfFile(string fileName, string catalogName)
        {
            using (CatalogSyncDbContext context = new CatalogSyncDbContext(fileName))
            {
                var mailbox = context.Mailboxes.ToList();
                context.Mailboxes.Remove(mailbox[0]);
                int i = context.SaveChanges();
                Debug.WriteLine(string.Format("Delete {0} record.", i));
                SqlConnection.ClearPool(context.Database.Connection as SqlConnection);
            }
        }

        [TestMethod]
        public void MultiThreadUpdate()
        {
            var catalogWithInitCatalog = Path.Combine(MdfDbFolder, "MultiThreadTest");
            if (!Directory.Exists(catalogWithInitCatalog))
            {
                Directory.CreateDirectory(catalogWithInitCatalog);
            }
            var file = Path.Combine(catalogWithInitCatalog, string.Format("{0}.mdf", DateTime.Now.ToString("yyyyMMddHHmmss")));
            CatalogSyncDbContext context = new CatalogSyncDbContext(file);

            Parallel.For(0, 2, (index) =>
             {
                 Thread.Sleep(index * 1000);

                 try
                 {
                     MailboxSyncModel model = new MailboxSyncModel()
                     {
                         Id = DateTime.Now.ToString("yyyyMMddHHmmss"),
                         DisplayName = "test",
                         MailAddress = string.Format("test{0}@arcserve.com", DateTime.Now.ToString("yyyyMMddHHmmss")),
                         SyncStatus = string.Empty,
                         Name = "test"

                     };
                     context.Mailboxes.Add(model);
                     context.SaveChanges();
                 }
                 catch(Exception e)
                 {

                 }
             });
            
        }

    }
}
