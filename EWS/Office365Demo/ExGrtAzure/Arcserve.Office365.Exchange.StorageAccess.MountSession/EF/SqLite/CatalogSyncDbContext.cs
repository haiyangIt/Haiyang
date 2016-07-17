using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.SqLite
{
    public class CatalogSyncDbContext : CatalogDbContextBase
    {
        public CatalogSyncDbContext(string fileName)
            : base(new SQLiteConnection() { ConnectionString = GetConnectStr(fileName) }, true)
        { }

        public static string GetConnectStr(string fileName)
        {
            //fileName = fileName.Replace("\\\\", "file://");
            //fileName = fileName.Replace("\\", "/");

            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = fileName;
            builder.ForeignKeys = true;
            return builder.ToString();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<CatalogSyncDbContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public static bool IsInFolder(Int64 id)
        {
            return id >= CatalogDbInitialize.FolderStartIndex && id < CatalogDbInitialize.ItemStartIndex;
        }

        public static IdType GetIdType(Int64 id)
        {
            if(id == CatalogDbInitialize.MailStartIndex)
            {
                return IdType.Root;
            }
            if (id > CatalogDbInitialize.MailStartIndex && id < CatalogDbInitialize.FolderStartIndex)
            {
                return IdType.Mailbox;
            }
            else if (id > CatalogDbInitialize.FolderStartIndex && id < CatalogDbInitialize.ItemStartIndex)
            {
                return IdType.Folder;
            }
            else if (id > CatalogDbInitialize.ItemStartIndex)
                return IdType.Item;
            else
                throw new NotSupportedException();
        }
    }

    public enum IdType
    {
        Root,
        Mailbox,
        Folder,
        Item
    }

    public class CatalogDbInitialize : SqliteCreateDatabaseIfNotExists<CatalogSyncDbContext>
    {
        public CatalogDbInitialize(DbModelBuilder modelBuilder) : base(modelBuilder) { }
        public CatalogDbInitialize(DbModelBuilder modelBuilder, bool nullByteFileMeansNotExisting) : base(modelBuilder, nullByteFileMeansNotExisting)
        {
        }

        public const Int64 MailStartIndex = 1;
        public const Int64 FolderStartIndex = 1000000000;
        public const Int64 ItemStartIndex = 10000000000000;

        protected override void Seed(CatalogSyncDbContext context)
        {
            var initMailbox = new Data.MailboxSyncModel()
            {
                UniqueId = MailStartIndex,
                ChangeKey = "",
                ChildFolderCount = 0,
                DisplayName = "TestMailbox",
                FolderTree = "",
                MailAddress = "Test@arcserve.com",
                Name = "TestMailbox",
                RootFolderId = "0",
                SyncStatus = ""
            };

            var initFolder = new Data.FolderSyncModel()
            {
                UniqueId = FolderStartIndex,
                FolderId = "TEST",
                ParentFolderId = "0",
                ChangeKey = "0",
                ChildFolderCount = 0,
                ChildItemCount = 0,
                DisplayName = "TEST",
                Id = "TEST",
                MailboxAddress = "TEST@ARCSERVE.com",
                FolderType = "IPF.NOTE",
                Location = "TEST",
                SyncStatus = "0",
                MailboxId = "TEST"
            };
            context.Folders.Add(initFolder);

            var initItem = new Data.ItemSyncModel()
            {
                UniqueId = ItemStartIndex,
                ActualSize = 0,
                ChangeKey = "0",
                CreateTime = DateTime.MinValue,
                DisplayName = "TESTMAIL",
                IsRead = true,
                ItemClass = "IPM.NOTE",
                Location = "TESTMAIL",
                MailboxAddress = "TEST@ARCSERVE.com",
                ParentFolderId = "TEST",
                Size = 0,
                SyncStatus = "0",
                ItemId = "TESTMAIL"
            };
            context.Items.Add(initItem);
            context.SaveChanges();

            context.Folders.Remove(initFolder);
            context.Items.Remove(initItem);
            context.SaveChanges();
            base.Seed(context);
        }
    }
}
