using EwsFrame;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace SqlDbImpl.Storage
{
    public class BlobDataAccessWithSizeLimitation
    {
        private static int _blobMaxSize;
        private static int BlobMaxSize
        {
            get
            {
                if (_blobMaxSize == 0)
                {
                    if(!FactoryBase.IsRunningOnAzure())
                    {
                        _blobMaxSize = 2 * 1024 * 1024;
                    }
                    else
                        _blobMaxSize = 4 * 1024 * 1024;
                }
                return _blobMaxSize;
            }
        }
        private const int BlobMaxNumber = 50000;
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


        public void ResetAllBlob(string organization, bool isResetAll = false)
        {
            string prefix = CatalogFactory.Instance.GetServiceContext().GetOrganizationPrefix();
            IEnumerable<CloudBlobContainer> containers = null;
            if (isResetAll)
                containers = _blobClient.ListContainers();
            else
                containers = _blobClient.ListContainers(prefix);
            foreach(var container in containers)
            {
                container.Delete();
            }
        }

        public BlobDataAccessWithSizeLimitation(CloudBlobClient blobClient)
        {
            _blobClient = blobClient;
        }
        private CloudBlobClient _blobClient;

        public void SaveBlob(string containerName, string blobNamePrefix, Stream data, bool isAdd = true)
        {
            containerName = ValidateContainerName(containerName, false);
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            long dataLength = data.Length;
            int index = 0;
            byte[] buffer = new byte[BlobMaxSize];
            int actualReadCount = 0;
            while (dataLength > 0)
            {
                string blobName = GetBlobName(blobNamePrefix, index);
                blobName = ValidateBlobName(blobName, false);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
                if (!blockBlob.Exists())
                {
                    actualReadCount = data.Read(buffer, 0, BlobMaxSize);
                    blockBlob.UploadFromByteArray(buffer, 0, actualReadCount);
                }
                else
                {
                    if (!isAdd)
                    {
                        blockBlob.Delete();
                        actualReadCount = data.Read(buffer, 0, BlobMaxSize);
                        blockBlob.UploadFromByteArray(buffer, 0, actualReadCount);
                    }
                    LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.WARN, "blob exist", "blob {0} in container {1} exists", blobName, containerName);
                }

                dataLength -= BlobMaxSize;
                index++;
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

        public void GetBlob(string containerName, string blobNamePrefix, Stream writeData, int size)
        {
            long dataLength = size;

            GetBlob(containerName, blobNamePrefix, writeData,
                () =>
                {
                    if (dataLength <= 0)
                        return true;
                    dataLength -= BlobMaxSize;
                    return false;
                }
                );
        }

        private delegate bool IsOutofRange();
        private void GetBlob(string containerName, string blobNamePrefix, Stream writeData, IsOutofRange outOfRangeFunc)
        {
            containerName = ValidateContainerName(containerName, false);
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            if (!container.Exists())
            {
                throw new FileNotFoundException(string.Format("container {0} can not found .", container));
            }

            int index = 0;
            byte[] buffer = new byte[BlobMaxSize];
            int actualSize = BlobMaxSize;
            while (actualSize == BlobMaxSize)
            {
                if (outOfRangeFunc())
                    break;

                string blobName = GetBlobName(blobNamePrefix, index);
                blobName = ValidateBlobName(blobName, false);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
                if (!blockBlob.Exists())
                    break;

                Stream reader = blockBlob.OpenRead();
                actualSize = reader.Read(buffer, 0, BlobMaxSize);
                writeData.Write(buffer, 0, actualSize);

                index++;
            }
        }

        public void GetBlob(string containerName, string blobNamePrefix, Stream writeData)
        {
            GetBlob(containerName, blobNamePrefix, writeData, () => { return false; });
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
            return currentContainerCount + GetBlobCount(itemSize) < BlobMaxNumber;
        }

        public static int GetBlobCount(int itemSize)
        {
            return (int)Math.Ceiling((double)itemSize / (double)BlobMaxSize);
        }

        internal static string GetBlobName(string blobNamePrefix, int index)
        {
            return string.Format("{0}{2}{1}", blobNamePrefix, index, BlobDataAccess.DashChar);
        }
    }
}