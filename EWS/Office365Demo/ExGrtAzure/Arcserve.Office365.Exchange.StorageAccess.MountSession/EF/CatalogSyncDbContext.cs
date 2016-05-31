using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF
{
    [DbConfigurationType(typeof(CustomApplicationDbConfiguration))]
    public class CatalogSyncDbContext : DbContext
    {
        public CatalogSyncDbContext(string file) : base(GetConnectionString(file, false, string.Empty))
        {

        }

        private CatalogSyncDbContext(string file, string initCatalog) : base(GetConnectionString(file, initCatalog))
        {

        }

        private CatalogSyncDbContext(string file, bool isInitCatalog, string initCatalogName) : base(GetConnectionString(file, isInitCatalog, initCatalogName)) { }

        private CatalogSyncDbContext(string file, bool initCatalog) : base(GetConnectionString(file, initCatalog))
        {

        }

        private static string GetConnectionString(string filePath, bool initCatalog, string initCatalogName)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            if (initCatalog)
                sqlBuilder.InitialCatalog = initCatalogName;

            sqlBuilder.DataSource = @"(LocalDb)\MSSQLLocalDB";
            sqlBuilder.AttachDBFilename = filePath;
            sqlBuilder.IntegratedSecurity = true;

            return sqlBuilder.ToString();
        }

        private static string GetConnectionString(string filePath, bool initCatalog)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            //sqlBuilder.InitialCatalog = initCatalog;

            sqlBuilder.DataSource = @"(LocalDb)\MSSQLLocalDB";
            sqlBuilder.AttachDBFilename = filePath;
            sqlBuilder.IntegratedSecurity = true;

            return sqlBuilder.ToString();
        }

        private static string GetConnectionString(string filePath, string initCatalog)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            sqlBuilder.InitialCatalog = initCatalog;

            sqlBuilder.DataSource = @"(LocalDb)\MSSQLLocalDB";
            sqlBuilder.AttachDBFilename = filePath;
            sqlBuilder.IntegratedSecurity = true;

            return sqlBuilder.ToString();
        }

        private static string GetConnectionString(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            sqlBuilder.InitialCatalog = Path.GetFileNameWithoutExtension(filePath);

            sqlBuilder.DataSource = @"(LocalDb)\MSSQLLocalDB";
            sqlBuilder.AttachDBFilename = filePath;
            sqlBuilder.IntegratedSecurity = true;

            return sqlBuilder.ToString();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<CaseSensitiveAttribute, CaseSensitiveAttribute>(
            "CaseSensitive",
            (property, attributes) => attributes.Single()));
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<MailboxSyncModel> Mailboxes { get; set; }
        public DbSet<FolderSyncModel> Folders { get; set; }
        public DbSet<ItemSyncModel> Items { get; set; }

        //public DbSet<MailboxSyncModel> UpdateMailboxes { get; set; }
        //public DbSet<FolderSyncModel> UpdateFolders { get; set; }
        //public DbSet<ItemSyncModel> UpdateItems { get; set; }
    }
}
