using EwsFrame;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SqlDbImpl.Storage
{
    public class BlobDataAccess
    {
        public const string DashString = "-";
        public const char DashChar = '-';
        public static readonly char[] DashCharArray = DashString.ToCharArray();

        private static HashSet<char> _containerValidKey;
        private static HashSet<char> ContainerValidKey
        {
            get
            {
                if (_containerValidKey == null)
                {
                    _containerValidKey = new HashSet<char>();
                    for (char controlLower = '0'; controlLower <= '9'; controlLower++)
                    {
                        _containerValidKey.Add(controlLower);
                    }
                    for (char controlLower = 'a'; controlLower <= 'z'; controlLower++)
                    {
                        _containerValidKey.Add(controlLower);
                    }
                    _containerValidKey.Add(DashChar);

                }
                return _containerValidKey;
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="organization"></param>
        /// <param name="mailboxPrefix">if null or empty, reset all blob. else reset specific mailbox.</param>
        public void ResetAllBlob(string organization, string mailboxPrefix)
        {
            var isResetAll = string.IsNullOrEmpty(mailboxPrefix);
            IEnumerable<CloudBlobContainer> containers = null;
            if (isResetAll)
                containers = _blobClient.ListContainers();
            else
                containers = _blobClient.ListContainers(mailboxPrefix);
            foreach (var container in containers)
            {
                container.Delete();
            }
        }

        public BlobDataAccess(CloudBlobClient blobClient)
        {
            _blobClient = blobClient;
        }
        private CloudBlobClient _blobClient;


        public void SaveBlob(string containerName, string blobNamePrefix, Stream data, bool isAdd = true)
        {
            containerName = ValidateContainerName(containerName, false);
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            string blobName = GetBlobName(blobNamePrefix, 0);
            blobName = ValidateBlobName(blobName, false);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            if (!blockBlob.Exists())
            {
                blockBlob.UploadFromStream(data);
            }
            else
            {
                if (!isAdd)
                {
                    blockBlob.Delete();
                    blockBlob.UploadFromStream(data);
                }
                LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.WARN, "blob exist", "blob {0} in container {1} exists", blobName, containerName);
            }

        }

        public bool IsBlobExist(string containerName, string blobNamePrefix)
        {
            containerName = ValidateContainerName(containerName, false);
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            if (!container.Exists())
            {
                return false;
            }
            string blobName = GetBlobName(blobNamePrefix, 0);
            blobName = ValidateBlobName(blobName, false);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            if (!blockBlob.Exists())
                return false;

            return true;
        }

        public void GetBlob(string containerName, string blobNamePrefix, Stream writeData)
        {
            containerName = ValidateContainerName(containerName, false);
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            if (!container.Exists())
            {
                throw new FileNotFoundException(string.Format("container {0} can not found .", container));
            }

            string blobName = GetBlobName(blobNamePrefix, 0);
            blobName = ValidateBlobName(blobName, false);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            if (!blockBlob.Exists())
                throw new FileNotFoundException(string.Format("blob {0} can not found .", blobName));
            blockBlob.DownloadToStream(writeData);
        }

        public CloudBlockBlob GetBlockBlobObj(string containerName, string blobName)
        {
            containerName = ValidateContainerName(containerName, false);
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            blobName = ValidateBlobName(blobName, false);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            return blockBlob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="isCheck"></param>
        /// <remarks>
        /// <code>if (containerName.Equals("$root"))
        ///return; 

        ///if (!Regex.IsMatch(containerName, @"^[a-z0-9](([a-z0-9\-[^\-])){1,61}[a-z0-9]$"))
        /// throw new Exception("Invalid container name);
        /// </code>
        /// </remarks>
        /// <returns></returns>
        public static string ValidateContainerName(string containerName, bool isCheck = true)
        {
            containerName = containerName.ToLower();
            if (containerName.Length < 3 || containerName.Length > 63)
                throw new ArgumentException(string.Format("container name {0} length {1} must be between 3 and 63. ", containerName, containerName.Length), "containerName");

            if (!isCheck)
                return containerName;

            bool previousIsDashChar = false;
            foreach (var c in containerName)
            {
                if (!ContainerValidKey.Contains(c))
                    throw new ArgumentException(string.Format("container name {0} contain invalid character. ", containerName, c), "containerName");
                if (previousIsDashChar && c == DashChar)
                {
                    throw new ArgumentException(string.Format("container name {0} contain consecutive dashes. ", containerName), "containerName");
                }
                if (c == DashChar)
                {
                    previousIsDashChar = true;
                }
                else
                {
                    previousIsDashChar = false;
                }
            }

            if (containerName[0] == DashChar)
                throw new ArgumentException(string.Format("container name {0} must start with a letter or number. ", containerName), "containerName");

            return containerName;
        }

        public static string ValidateBlobName(string blobName, bool isCheck = true)
        {
            if (blobName.Length < 1 || blobName.Length > 1024)
                throw new ArgumentException(string.Format("blobName name {0} length {1} must be between 1 and 1024. ", blobName, blobName.Length), "containerName");

            if (!isCheck)
                return blobName;

            // todo need more validation.
            return blobName;
        }

        public static bool IsNotOutOfBlobCountRange(int currentContainerCount, int itemSize)
        {
            return false;
        }

        public static int GetBlobCount(int itemSize)
        {
            return 1;
        }

        internal static string GetBlobName(string blobNamePrefix, int index)
        {
            return string.Format("{0}{2}{1}", blobNamePrefix, index, BlobDataAccess.DashChar);
        }

        public string GetSharedUri(string containerName)
        {
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            string sasContainerToken = container.GetSharedAccessSignature(GetSASPolicy());
            return container.Uri + sasContainerToken;
        }

        private SharedAccessBlobPolicy GetSASPolicy()
        {
            //Create a new shared access policy and define its constraints.
            SharedAccessBlobPolicy sharedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(Config.RestoreCfgInstance.SASExpireHours),
                Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read,

                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(Config.RestoreCfgInstance.SASStartTimeMinute)
            };
            return sharedPolicy;
        }

        public List<string> GetBlobShareUris(string containerName, List<string> blobNames)
        {
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            string sasContainerToken = container.GetSharedAccessSignature(GetSASPolicy());

            List<string> blobResult = new List<string>(blobNames.Count);

            StringBuilder sb = new StringBuilder(container.Uri.AbsoluteUri.Length + sasContainerToken.Length + 30);
            foreach(var name in blobNames)
            {
                sb.Append(container.Uri.AbsoluteUri);
                sb.Append("/");
                sb.Append(name);
                sb.Append(sasContainerToken);
                blobResult.Add(sb.ToString());
                sb.Length = 0;
            }

            return blobResult;
        }

        public string GetBlobSharedUri(string containerName, string blobName)
        {
            containerName = ValidateContainerName(containerName, false);
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            blobName = ValidateBlobName(blobName, false);
            var blob = container.GetBlobReferenceFromServer(blobName);

            string sasContainerToken = blob.GetSharedAccessSignature(GetSASPolicy());

            return blob.Uri.AbsoluteUri + sasContainerToken;



        }
    }
}