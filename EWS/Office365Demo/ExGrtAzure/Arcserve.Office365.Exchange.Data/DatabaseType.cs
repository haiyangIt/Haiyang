using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data
{
    public enum DatabaseType
    {
        SqLite = 1,
        SqlServer = 2
    }

    public static class DatabaseTypeExtension
    {
        public static Dictionary<string, DatabaseType> _dic = new Dictionary<string, DatabaseType>
        {
            {"SqLite", DatabaseType.SqLite },
            {"SqlServer", DatabaseType.SqlServer }
        };
        public static DatabaseType GetDatabaseType(this string dbType)
        {
            return _dic[dbType];
        }

        public const string SqlServerDbFile = "Catalog.mdf";
        public const string SqlServerLogFile = "Catalog_log.ldf";
        public const string SqLiteDbFile = "Catalog.sqlite";
        public static string GetCatalogDatabaseFileName(this string organizationName, DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return SqlServerDbFile;
                case DatabaseType.SqLite:
                    return SqLiteDbFile;
                default:
                    throw new NotSupportedException();
            }
            
        }

        public static void CatalogFileCopy(this string sourceFolder, string desFolder, DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    var mdFileName = Path.Combine(sourceFolder, SqlServerDbFile);
                    var logFileName = Path.Combine(sourceFolder, SqlServerLogFile);
                    var desMdfFileName = Path.Combine(desFolder, SqlServerDbFile);
                    var deslogFileName = Path.Combine(desFolder, SqlServerLogFile);
                    if (!Directory.Exists(desFolder))
                    {
                        Directory.CreateDirectory(desFolder);
                    }
                    File.Copy(mdFileName, desMdfFileName);
                    File.Copy(logFileName, deslogFileName);
                    break;
                case DatabaseType.SqLite:
                    var sqliteFileName = Path.Combine(sourceFolder, SqLiteDbFile);
                    var desSqliteFileNamee = Path.Combine(desFolder, SqLiteDbFile);
                    if (!Directory.Exists(desFolder))
                    {
                        Directory.CreateDirectory(desFolder);
                    }
                    File.Copy(sqliteFileName, desSqliteFileNamee);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
