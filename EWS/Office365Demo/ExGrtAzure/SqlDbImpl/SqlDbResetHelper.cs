using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SqlDbImpl.Model;
using SqlDbImpl.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDbImpl
{
    public class SqlDbResetHelper
    {
        public void ResetBlobData(string organization, bool isResetAll = false)
        {
            CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
  ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();
            BlobDataAccess blobDataAccess = new BlobDataAccess(BlobClient);
            blobDataAccess.ResetAllBlob(organization, isResetAll);
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
