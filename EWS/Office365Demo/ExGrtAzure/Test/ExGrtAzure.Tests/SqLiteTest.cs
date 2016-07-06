using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using System.Data.SQLite;
using SQLite.CodeFirst;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Arcserve.Office365.Exchange.Data;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class SqLiteTest
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

        [TestMethod]
        public void TestMethod1()
        {
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

            var mdfFileName = "Test.sqlite";
            var sourMdfFile = Path.Combine(sourMdf, mdfFileName);
            var desMdfFile = Path.Combine(desMdf, mdfFileName);

            CreateMdfFile(sourMdfFile, false);
            //System.Data.SqlClient.SqlConnection.ClearAllPools();
            File.Copy(sourMdfFile, desMdfFile);

            CreateMdfFile(desMdfFile, false);
            AsscessMdfFile(sourMdfFile, false);
            AsscessMdfFile(desMdfFile, false);

            DeleteMdfFile(desMdfFile);
            AsscessMdfFile(desMdfFile, false);
        }

        private void CreateMdfFile(string fileName, bool isInitCatalog)
        {
            using (MyDbContext context = new MyDbContext(fileName))
            {
                MailboxSyncModelTest model = new MailboxSyncModelTest()
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

        }

        private void AsscessMdfFile(string fileName, bool isInitCatalog)
        {
            using (MyDbContext context = new MyDbContext(fileName))
            {
                var mailbox = context.Mailboxes;
                foreach (var m in mailbox)
                {
                    Debug.WriteLine(m.MailAddress);
                }
            }
        }

        private void DeleteMdfFile(string fileName)
        {
            using (MyDbContext context = new MyDbContext(fileName))
            {
                var mailbox = context.Mailboxes.ToList();
                context.Mailboxes.Remove(mailbox[0]);
                int i = context.SaveChanges();
                Debug.WriteLine(string.Format("Delete {0} record.", i));
            }
        }
    }

    [DbConfigurationType(typeof(CustomApplicationDbConfiguration))]
    public class MyDbContext : DbContext
    {
        public MyDbContext(string fileName)
            : base(new SQLiteConnection() { ConnectionString = GetConnectStr(fileName) }, true)
        { }

        public static string GetConnectStr(string fileName)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = fileName;
            builder.ForeignKeys = true;
            return builder.ToString();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<MyDbContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public DbSet<MailboxSyncModelTest> Mailboxes { get; set; }
    }

    [Table("MailboxSync")]
    public class MailboxSyncModelTest
    {
        public MailboxSyncModelTest()
        {

        }

        public string FolderTree { get; set; }

        [NotMapped]
        public string ChangeKey
        {
            get; set;
        }

        [NotMapped]
        public int ChildFolderCount
        {
            get; set;
        }

        [MaxLength(255)]
        public string DisplayName
        {
            get; set;
        }

        [Index]
        public string Id
        {
            get; set;
        }

        
        [NotMapped]
        public string Location
        {
            get
            {
                return MailAddress;
            }
            set { }
        }

        private string _mailAddress;
        [MaxLength(255)]
        [EmailAddress]
        public string MailAddress
        {
            get
            {
                return _mailAddress;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _mailAddress = string.Empty;
                else
                    _mailAddress = value.ToLower();
            }
        }

        public string Name
        {
            get; set;
        }

        [NotMapped]
        [MaxLength(512)]
        [CaseSensitive]
        public string RootFolderId
        {
            get; set;
        }

        /// <summary>
        /// FolderHierarchy status. 
        /// </summary>
        [CaseSensitive]
        public string SyncStatus
        {
            get; set;
        }

        [Key]
        public Int64 UniqueId
        {
            get; set;
        }
    }
}
