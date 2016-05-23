using Arcserve.Office365.Exchange.EwsApi.Increment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using System.IO;
using Arcserve.Office365.Exchange.Util;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession
{
    public class ExportItemWriter : IExportItemsOper, IDisposable
    {
        private Dictionary<string, FileStream> _itemFileStream = new Dictionary<string, FileStream>();
        private string _workFolder;
        public ExportItemWriter(string workFolder)
        {
            _workFolder = workFolder;
        }

        public void Dispose()
        {
            foreach (var keyValue in _itemFileStream)
            {
                if (keyValue.Value != null)
                {
                    keyValue.Value.Dispose();
                    _itemFileStream[keyValue.Key] = null;
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
                using (_itemFileStream.LockWhile(() =>
                {
                    if (!_itemFileStream.TryGetValue(item.ItemId, out fileStream))
                    {
                        var folder = CreateDirectory(_workFolder, item);
                        var file = Path.Combine(folder, string.Format("{0}.bin", item.GetFileName()));

                        fileStream = new FileStream(file, FileMode.CreateNew);
                        _itemFileStream.Add(item.ItemId, fileStream);
                    }
                }))
                { }
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
            }

        }
    }
}
