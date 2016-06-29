using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF
{
    public abstract class CatalogDbContextBase : DbContext
    {
        protected CatalogDbContextBase(DbConnection connect, bool isOwnConnection) : base(connect, isOwnConnection)
        {

        }

        protected CatalogDbContextBase(string connection) : base(connection)
        {

        }

        public DbSet<MailboxSyncModel> Mailboxes { get; set; }
        public DbSet<FolderSyncModel> Folders { get; set; }
        public DbSet<ItemSyncModel> Items { get; set; }

        public static CatalogDbContextBase NewCatalogContext(string fileName)
        {
            if(CloudConfig.Instance.DbType == "Sql Server")
            {
                return new SqlServer.CatalogSyncDbContext(fileName);
            }
            else
            {
                return new SqLite.CatalogSyncDbContext(fileName);
            }
        }
    }
}
