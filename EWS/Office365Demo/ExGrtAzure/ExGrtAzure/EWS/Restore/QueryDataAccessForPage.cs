using ExGrtAzure.EWS.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExGrtAzure.EWS.MailboxOperator;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ExGrtAzure.EWS.Restore
{
    public class QueryDataAccessForPage : IQueryDataAccessForPage
    {
        private static CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
    ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

        private static CloudTableClient TableClient = StorageAccount.CreateCloudTableClient();

        private static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

        public List<ICatalogJob> GetCatalogJobs(string organization, PageInfo page)
        {
            throw new NotImplementedException();
        }

        public List<IFolderData> GetFolders(string organization, string mailboxAddress, DateTime catalogStartTime, PageInfo pageInfo)
        {
            throw new NotImplementedException();
        }

        public List<IItemData> GetItems(string organization, IFolderData folderInfo, DateTime catalogStartTime, PageInfo pageInfo)
        {
            throw new NotImplementedException();
        }

        public List<IMailboxData> GetMailboxes(string organization, DateTime catalogStartTime, PageInfo pageInfo)
        {
            throw new NotImplementedException();
        }
    }
}