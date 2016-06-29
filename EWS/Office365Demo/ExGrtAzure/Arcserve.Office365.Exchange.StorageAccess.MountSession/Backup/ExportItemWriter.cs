using System;
using System.Collections.Generic;
using Arcserve.Office365.Exchange.Data.Increment;
using System.IO;
using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.Backup
{
    public class ExportItemWriter : IExportItemsOper, IDisposable
    {

        private Dictionary<string, FileStream> _itemFileStream = new Dictionary<string, FileStream>();
        private string _workFolder;
        public ExportItemWriter(string workFolder)
        {
            if (!string.IsNullOrEmpty(CloudConfig.Instance.WorkFolder))
            {
                _workFolder = CloudConfig.Instance.WorkFolder;
            }
            else
                _workFolder = workFolder;
        }

        public void Dispose()
        {
            foreach (var keyValue in _itemFileStream)
            {
                if (keyValue.Value != null)
                {
                    keyValue.Value.Dispose();
                }
            }
        }

        public void ExportItemError(EwsResponseException ewsResponseError)
        {
            Log.LogFactory.LogInstance.WriteException("ExportItems", Log.LogLevel.ERR,
                string.Format("Export item {0} {1} size {2} error.", ewsResponseError.Item.ItemId, ewsResponseError.Item.DisplayName, ewsResponseError.Item.Size),
                ewsResponseError, ewsResponseError.Message);
        }

        public void WriteBufferToStorage(IItemDataSync item, byte[] buffer, int length)
        {
            FileStream fileStream = null; ;

            if (!_itemFileStream.TryGetValue(item.ItemId, out fileStream))
            {
                var file = item.GetFilePath(_workFolder);
                var folder = Path.GetDirectoryName(file);
                try {
                    if (!Directory.Exists(folder))
                    {

                        Directory.CreateDirectory(folder);
                    }

                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch(Exception e)
                {
                    LogFactory.LogInstance.WriteException(LogLevel.ERR, "Create file error", e, string.Format("Directory:[{0}] File:[{1}]", folder, file));
                    throw new ArgumentException(string.Format("Directory:[{0}] File:[{1}]", folder, file), e);
                }

                fileStream = new FileStream(file, FileMode.CreateNew);
                _itemFileStream.Add(item.ItemId, fileStream);
            }
            fileStream.Write(buffer, 0, length);
        }


        private static object _directoryLock = new object();
        private string CreateDirectory(string folder, IItemDataSync item)
        {
            var location = item.Location;
            var displayNames = location.GetFolderDisplays();

            var folderPath = Path.Combine(folder, item.MailboxAddress.GetValidFolderName());

            foreach (var displayName in displayNames)
            {
                var folderName = displayName.GetValidFolderName();
                folderPath = Path.Combine(folderPath, folderName);
                if (!Directory.Exists(folderPath))
                {
                    using (_directoryLock.LockWhile(() =>
                {
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                }))
                    { }
                }
            }
            return folderPath;

        }

        public void WriteComplete(IItemDataSync item)
        {
            FileStream fileStream = null; ;

            if (_itemFileStream.TryGetValue(item.ItemId, out fileStream))
            {
                fileStream.Dispose();
                fileStream = null;
                _itemFileStream.Remove(item.ItemId);
            }

        }
    }
}
