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
            fileName = fileName.Replace("\\\\", "file://");
            fileName = fileName.Replace("\\", "/");
            
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
    }
}
