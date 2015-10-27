using EwsDataInterface;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TableBlobImpl.Access.Table;

namespace TableBlobImpl.Storage.Table.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Catalog job information: table name is organization name + job info.
    /// Contains: catalog job name and start time two colunm. these two column can get from rowkey and partitionkey.
    /// Rowkeys: catalog job name,
    /// PartitionKeys: starttime.ticks.
    /// </remarks>
    public class CatalogEntity : TableEntity, ICatalogJob
    {
        [IgnoreProperty()]
        public DateTime StartTime { get; set; }

        public string CatalogJobName { get; set; }

        [IgnoreProperty]
        public string Organization { get; set; }

        public static void SetPartitionRowKeys(CatalogEntity entity)
        {
            entity.PartitionKey = TableDataAccess.ValidateRowPartitionKey(entity.Organization, true);
            long ticks = DateTime.MaxValue.Ticks - entity.StartTime.Ticks;
            entity.RowKey = TableDataAccess.ValidateRowPartitionKey(ticks.ToString(), false);
        }

        public static void SetOtherByPartitionRowKeys(CatalogEntity entity)
        {
            entity.StartTime = new DateTime(DateTime.MaxValue.Ticks - Convert.ToInt64(entity.RowKey));
            entity.Organization = entity.RowKey;
        }
        internal static string GetCatalogJobTableName(string orignizeName)
        {
            return "catalogjobinformation";
            //return string.Format("catalogjobinformation", orignizeName);
        }
    }
}