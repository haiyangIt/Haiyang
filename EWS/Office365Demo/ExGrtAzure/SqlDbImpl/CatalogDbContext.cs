using SqlDbImpl.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDbImpl
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(string organizationName, SqlConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
            Organization = organizationName;

            Database.SetInitializer<CatalogDbContext>(new CustomerCatalogDbInitializer());
        }

        public CatalogDbContext(string organizationName) : base(GetConnectString(organizationName))
        {
            Organization = organizationName;

            Database.SetInitializer<CatalogDbContext>(new CustomerCatalogDbInitializer());
        }

        public static string GetConnectString(string organizationName)
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            sqlBuilder.DataSource = ConfigurationManager.AppSettings["DataSource"];
            sqlBuilder.UserID = ConfigurationManager.AppSettings["UserId"];
            sqlBuilder.Password = ConfigurationManager.AppSettings["Password"];
            sqlBuilder.PersistSecurityInfo = Convert.ToBoolean(ConfigurationManager.AppSettings["PersistSecurityInfo"]);
            sqlBuilder.IntegratedSecurity = false;
            sqlBuilder.InitialCatalog = organizationName;
            return sqlBuilder.ToString();
        }

        public static string GetEntityConnectString(string organizationName)
        {
            
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = ConfigurationManager.AppSettings["ProviderName"];

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = GetConnectString(organizationName);

            // Set the Metadata location.
            entityBuilder.Metadata = @"res://*/AdventureWorksModel.csdl|
                            res://*/AdventureWorksModel.ssdl|
                            res://*/AdventureWorksModel.msl";

            return entityBuilder.ToString();
        }

        public string Organization { get; private set; }

        public DbSet<CatalogInfoModel> Catalogs { get; set; }
        public DbSet<MailboxModel> Mailboxes { get; set; }
        public DbSet<FolderModel> Folders { get; set; }
        public DbSet<ItemModel> Items { get; set; }
        public DbSet<ItemLocationModel> ItemLocations { get; set; }
    }

    public class CustomerCatalogDbInitializer : CreateDatabaseIfNotExists<CatalogDbContext>
    {
        public override void InitializeDatabase(CatalogDbContext context)
        {
            if (!context.Database.Exists())
            {
                base.InitializeDatabase(context);
            }

            if (!context.Database.CompatibleWithModel(true))
            {
                throw new NotSupportedException("The model is not compatible with the database.");
            }
        }
    }
}
