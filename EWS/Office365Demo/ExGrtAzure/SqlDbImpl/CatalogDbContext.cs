using EwsFrame.EF;
using SqlDbImpl.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDbImpl
{
    [DbConfigurationType(typeof(CustomApplicationDbConfiguration))]
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext() : base("name=Organization")
        {
            Database.SetInitializer<CatalogDbContext>(new CustomerCatalogDbInitializer());
        }

        public CatalogDbContext(OrganizationModel organization, SqlConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
            Organization = organization;

            Database.SetInitializer<CatalogDbContext>(new CustomerCatalogDbInitializer());
        }

        public CatalogDbContext(string connectString): base(connectString)
        {
            Database.SetInitializer<CatalogDbContext>(new CustomerCatalogDbInitializer());
        }

        public CatalogDbContext(OrganizationModel organization) : base(GetConnectString(organization.Name))
        {
            Organization = organization;

            Database.SetInitializer<CatalogDbContext>(new CustomerCatalogDbInitializer());
        }

        public static string GetConnectString(string organizationName)
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            sqlBuilder.InitialCatalog = organizationName;
            return sqlBuilder.ToString();
        }

        //public static string GetEntityConnectString(string organizationName)
        //{
            
        //    EntityConnectionStringBuilder entityBuilder =
        //        new EntityConnectionStringBuilder();

        //    //Set the provider name.
        //    entityBuilder.Provider = ConfigurationManager.AppSettings["ProviderName"];

        //    // Set the provider-specific connection string.
        //    entityBuilder.ProviderConnectionString = GetConnectString(organizationName);

        //    // Set the Metadata location.
        //    entityBuilder.Metadata = @"res://*/AdventureWorksModel.csdl|
        //                    res://*/AdventureWorksModel.ssdl|
        //                    res://*/AdventureWorksModel.msl";

        //    return entityBuilder.ToString();
        //}

        public OrganizationModel Organization { get; private set; }

        public DbSet<CatalogInfoModel> Catalogs { get; set; }
        public DbSet<MailboxModel> Mailboxes { get; set; }
        public DbSet<FolderModel> Folders { get; set; }
        public DbSet<ItemModel> Items { get; set; }
        public DbSet<ItemLocationModel> ItemLocations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<CaseSensitiveAttribute, CaseSensitiveAttribute>(
            "CaseSensitive",
            (property, attributes) => attributes.Single()));
            base.OnModelCreating(modelBuilder);
        }
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
