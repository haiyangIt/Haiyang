using Arcserve.Office365.Exchange.Azure;
using Arcserve.Office365.Exchange.Data.Impl.Mail;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.StorageAccess.Azure
{
    public class SqlDbResetHelper
    {
        public void ResetBlobData(string organization, string mailboxPrefix)
        {
            CloudStorageAccount StorageAccount = FactoryBase.GetStorageAccount();

            CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();
            BlobDataAccess blobDataAccess = new BlobDataAccess(BlobClient);
            blobDataAccess.ResetAllBlob(organization, mailboxPrefix);
        }

        public void DeleteDatabase(string organization)
        {
            using (CatalogDbContext context = new CatalogDbContext(new OrganizationModel() { Name = organization }))
            {
                if(context.Database.Exists())
                {
                    context.Database.Delete();
                }
            }
        }
    }
}
