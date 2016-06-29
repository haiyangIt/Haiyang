using Arcserve.Office365.Exchange.Tool.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.DataProtect.Tool.Result;
using System.IO;
using System.Data.SqlClient;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF;
using System.Diagnostics;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Data;

namespace Arcserve.Office365.Exchange.DataProtect.Tool.Command
{
    public class TestCommand : ArcServeCommand
    {
        public const string CommandName = "Test";

        public TestCommand(CommandArgs args) : base(args)
        {

        }

        protected override ResultBase DoExcute()
        {
            CreateMdfWithOutInitCatalog();
            return new ExchangeBackupResult();
        }

        protected override ResultBase GetErrorResultBase(Exception e)
        {
            return new ExchangeBackupResult(e.Message);
        }

        protected override ResultBase GetInvalidUserPsw()
        {
            return new ExchangeBackupResult();
        }

        private string MdfDbFolder
        {
            get
            {
                var result = Path.Combine(@"\\Jacky-RPS01\CA_UDP_NON-0000\ArcserveJacky.onmicrosoft.com[575aa3e7-7035-4e6b-9011-39b43f8a0d70]\Catalog\S0000000004\GRT_1", "Db");
                //var result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Db");
                if (!Directory.Exists(result))
                {
                    Directory.CreateDirectory(result);
                }
                return result;
            }
        }

        public string GetDbFileName()
        {
            if (CloudConfig.Instance.DbType.GetDatabaseType() == DatabaseType.SqLite)
            {
                return "Arcserve.sqlite";
            }
            else
            {
                return "Arcserve.mdf";
            }
        }

        public void CopyFile(string sourMdf, string desMdf)
        {
            if (CloudConfig.Instance.DbType.GetDatabaseType() == DatabaseType.SqlServer)
            {
                var logFileName = "Arcserve_log.ldf";
                var sourLogFile = Path.Combine(sourMdf, logFileName);
                var defLogFile = Path.Combine(desMdf, logFileName);
                File.Copy(sourLogFile, defLogFile);
            }
        }

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
                System.Threading.Thread.Sleep(2000);
                Directory.CreateDirectory(sourMdf);
            }
            if (!Directory.Exists(desMdf))
            {
                Directory.CreateDirectory(desMdf);
            }
            else
            {
                Directory.Delete(desMdf, true);
                System.Threading.Thread.Sleep(2000);
                Directory.CreateDirectory(desMdf);
            }

            var mdfFileName = GetDbFileName();
            var catalogName = "Arcserve";
            var sourMdfFile = Path.Combine(sourMdf, mdfFileName);
            var desMdfFile = Path.Combine(desMdf, mdfFileName);

            CreateMdfFile(sourMdfFile, false, catalogName);
            //System.Data.SqlClient.SqlConnection.ClearAllPools();
            System.Threading.Thread.Sleep(10000);
            File.Copy(sourMdfFile, desMdfFile);
            CopyFile(sourMdf, desMdf);

            CreateMdfFile(desMdfFile, false, catalogName);
            AsscessMdfFile(sourMdfFile, false, catalogName);
            AsscessMdfFile(desMdfFile, false, catalogName);

            DeleteMdfFile(desMdfFile, catalogName);
            AsscessMdfFile(desMdfFile, false, catalogName);
        }

        private void CreateMdfFile(string fileName, bool isInitCatalog, string catalogName)
        {
            using (CatalogDbContextBase context = CatalogDbContextBase.NewCatalogContext(fileName))
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
                if (CloudConfig.Instance.DbType.GetDatabaseType() == DatabaseType.SqlServer)
                    SqlConnection.ClearPool(context.Database.Connection as SqlConnection);
            }

        }

        private void AsscessMdfFile(string fileName, bool isInitCatalog, string catalogName)
        {
            using (CatalogDbContextBase context = CatalogDbContextBase.NewCatalogContext(fileName))
            {
                var mailbox = context.Mailboxes;
                foreach (var m in mailbox)
                {
                    Debug.WriteLine(m.MailAddress);
                }
                if (CloudConfig.Instance.DbType.GetDatabaseType() == DatabaseType.SqlServer)
                    SqlConnection.ClearPool(context.Database.Connection as SqlConnection);
            }
        }

        private void DeleteMdfFile(string fileName, string catalogName)
        {
            using (CatalogDbContextBase context = CatalogDbContextBase.NewCatalogContext(fileName))
            {
                var mailbox = context.Mailboxes.ToList();
                context.Mailboxes.Remove(mailbox[0]);
                int i = context.SaveChanges();
                Debug.WriteLine(string.Format("Delete {0} record.", i));
                if (CloudConfig.Instance.DbType.GetDatabaseType() == DatabaseType.SqlServer)
                    SqlConnection.ClearPool(context.Database.Connection as SqlConnection);
            }
        }
    }
}
