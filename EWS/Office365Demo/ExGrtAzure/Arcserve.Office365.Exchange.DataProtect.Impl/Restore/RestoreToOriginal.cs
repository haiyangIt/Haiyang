using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Data;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Restore
{
    internal class RestoreToOriginal : RestoreToPositionBase
    {
        public RestoreToOriginal()
        {

        }

        internal RestoreToOriginal(IMailboxDataSync currentRestoreMailbox)
        {
            _currentRestoreMailbox = currentRestoreMailbox;
        }

        private IMailboxDataSync _currentRestoreMailbox;
        public override IRestoreToPosition<IJobProgress> NewRestoreToPosition(IMailboxDataSync currentRestoreMailbox)
        {
            var result = new RestoreToOriginal(currentRestoreMailbox);
            CloneBaseMembers(result);
            return result;
        }

        protected override void ConnectExchangeService(Func<string, OrganizationAdminInfo, string> funcConnectExchangeService)
        {
            funcConnectExchangeService.Invoke(_currentRestoreMailbox.MailAddress, AdminInfo);
        }

        protected override bool IsConnectExchangeService()
        {
            return true;
        }

        private Dictionary<string, IRestoreDestinationFolder> _folders = new Dictionary<string, IRestoreDestinationFolder>();
        public override IRestoreDestinationFolder GetAndCreateFolderIfFolderNotExist(IFolderDataSync folder, IRestoreDestinationFolder parentFolder)
        {
            IRestoreDestinationFolder result = null;
            if (!_folders.TryGetValue(folder.FolderId, out result))
            {
                if (parentFolder == null)
                {
                    var folderInService = ExchangeAccessForRestore.EwsServiceAdapter.FindFolderInRoot(((IItemBase)folder).DisplayName);
                    if(folderInService == null)
                    {
                        folderInService = ExchangeAccessForRestore.EwsServiceAdapter.CreateFolder(((IItemBase)folder).DisplayName, folder.FolderType);
                    }
                    result = new RestoreServerDestinationFolder(folderInService);
                }
                else
                {
                    var folderInService = ExchangeAccessForRestore.EwsServiceAdapter.FindFolder(((IItemBase)folder).DisplayName, parentFolder.Id);
                    if (folderInService == null)
                    {
                        folderInService = ExchangeAccessForRestore.EwsServiceAdapter.CreateFolder(((IItemBase)folder).DisplayName, folder.FolderType, parentFolder.Id);
                    }
                    result = new RestoreServerDestinationFolder(folderInService);
                }

                var key = string.Format("{0}_{1}", result.Id, result.Name);
                _folders.Add(result.Id, result);
            }

            return result;
        }

        public override void ImportItems(IEnumerable<ImportItemStatus> partition, IRestoreDestinationFolder folder)
        {
            ExchangeAccessForRestore.EwsServiceAdapter.ImportItems(partition, ((RestoreServerDestinationFolder)folder).Folder);
        }

        public override IEnumerable<string> GetNotExistItems(IRestoreDestinationFolder destinationFolder, IFolderDataSync folderInCatalog)
        {
            return new List<string>(0);
        }

        public override bool ImportExistItems()
        {
            throw new NotImplementedException();
        }

        public override bool ImportNotExistItems()
        {
            throw new NotImplementedException();
        }
    }

    internal class RestoreToOriginalSkipExist : RestoreToOriginal
    {
        public override IEnumerable<string> GetNotExistItems(IRestoreDestinationFolder destinationFolder, IFolderDataSync folderInCatalog)
        {
            List<string> result = new List<string>();
            ChangeCollection<ItemChange> syncResult = null;
            var syncStatus = folderInCatalog.SyncStatus;
            do {
                syncResult = this.ExchangeAccessForRestore.EwsServiceAdapter.SyncItems(((RestoreServerDestinationFolder)destinationFolder).Folder.Id, syncStatus);
                syncStatus = syncResult.SyncState;

                foreach(var changeItems in syncResult)
                {
                    if(changeItems.ChangeType == ChangeType.Delete)
                    {
                        result.Add(changeItems.ItemId.UniqueId);
                    }
                }
            } while (syncResult.MoreChangesAvailable);
            return result;
        }

        public override bool ImportExistItems()
        {
            return false;
        }

        public override bool ImportNotExistItems()
        {
            return true;
        }
    }

    internal class RestoreToOriginalOverwriteExist : RestoreToOriginal
    {

        public override bool ImportExistItems()
        {
            return true;
        }

        public override bool ImportNotExistItems()
        {
            return true;
        }
    }
}
