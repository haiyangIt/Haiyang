using EwsFrame.Util.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace EntityFrameworkTest.Models
{
    public class TestModel
    {
        [Key]
        public string TestId { get; set; }
        public string TestName { get; set; }
    }

    public class TestContext : DbContext
    {
        public TestContext(string dbName):base(GetConnectString(dbName))
        {
            Database.SetInitializer(new CustomInitializer());
            Database.Initialize(true);
        }

        public static string GetConnectString(string dbName)
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder(CloudConfig.Instance.DbConnectString);
            sqlBuilder.InitialCatalog = dbName;
            return sqlBuilder.ToString();
        }

        public DbSet<TestModel> TestModels { get; set; }
    }

    class CustomInitializer : IDatabaseInitializer<TestContext>
    {
        public void InitializeDatabase(TestContext context)
        {
            if (!context.Database.Exists() || !context.Database.CompatibleWithModel(false))
            {
                var configuration = new DbMigrationsConfiguration();
                var migrator = new DbMigrator(configuration);
                migrator.Configuration.TargetDatabase = new DbConnectionInfo(context.Database.Connection.ConnectionString, "System.Data.SqlClient");
                var migrations = migrator.GetPendingMigrations();
                if (migrations.Any())
                {
                    var scriptor = new MigratorScriptingDecorator(migrator);
                    string script = scriptor.ScriptUpdate(null, migrations.Last());
                    if (!String.IsNullOrEmpty(script))
                    {
                        context.Database.ExecuteSqlCommand(script);
                    }
                }
            }
        }
    }
}