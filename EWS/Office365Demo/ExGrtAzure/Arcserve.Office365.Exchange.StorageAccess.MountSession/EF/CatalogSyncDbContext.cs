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
        public CatalogSyncDbContext(string file) : base(GetConnectionString(file))
        {

        }

        private static string GetConnectionString(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            sqlBuilder.InitialCatalog = fileName;
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
    }
}
