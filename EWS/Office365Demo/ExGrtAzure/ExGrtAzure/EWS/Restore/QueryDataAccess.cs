using ExGrtAzure.EWS.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExGrtAzure.EWS.MailboxOperator;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using ExGrtAzure.EWS.Catalog.Storage;
using ExGrtAzure.EWS.Catalog;

namespace ExGrtAzure.EWS.Restore
{
    public class QueryDataAccess : IQueryDataAccess
    {
        private static CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
    ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

        private static CloudTableClient TableClient = StorageAccount.CreateCloudTableClient();

        private static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

        public List<ICatalogJob> GetAllCatalogJob(string organization)
        {
            string tableName = NameHelper.GetCatalogJobTableName(organization);
            tableName = TableDataAccess.ValidateTableName(tableName);
            TableDataAccess tableDataAccess = new TableDataAccess(TableClient);
            CloudTable table = tableDataAccess.GetTable(tableName);
            if (table == null)
                return null;

            TableQuery<CatalogEntity> query = new TableQuery<CatalogEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, organization));
            
            query.TakeCount = 100;
            TableRequestOptions a = new TableRequestOptions();
            OperationContext c = new OperationContext();
            
            var queryResult = table.ExecuteQuery(query);

            List<ICatalogJob> result = new List<ICatalogJob>(queryResult.Count());
            foreach (CatalogEntity entity in queryResult)
            {
                CatalogEntity.SetOtherByPartitionRowKeys(entity);
                result.Add(entity);
            }
            return result;

        }

        public List<IFolderData> GetAllFolder(string organization, DateTime catalogStartTime)
        {
            throw new NotImplementedException();
        }

        public List<IItemData> GetAllItem(string organization, IFolderData folderInfo, DateTime catalogStartTime)
        {
            throw new NotImplementedException();
        }

        public List<IMailboxData> GetAllMailbox(string organization, DateTime catalogStartTime)
        {
            throw new NotImplementedException();
        }
    }
}