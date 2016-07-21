using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Restore
{
    public interface IRestoreToPosition<ProgressType> : ITaskSyncContext<ProgressType>
    {
        IExchangeAccessForRestore<ProgressType> ExchangeAccessForRestore { get; set; }

        OrganizationAdminInfo AdminInfo { get; set; }

        void ConnectExchangeService();
        IRestoreDestinationFolder GetAndCreateFolderIfFolderNotExist(IFolderDataSync folder, IRestoreDestinationFolder parentFolder);
        void ImportItems(IEnumerable<ImportItemStatus> partition, IImportItemsOper importItemOper, IRestoreDestinationFolder folder);

        IRestoreToPosition<ProgressType> NewRestoreToPosition(IMailboxDataSync currentRestoreMailbox);
        IEnumerable<string> GetNotExistItems(IRestoreDestinationFolder destinationFolder, IFolderDataSync folderInCatalog);
        bool ImportExistItems();
        bool ImportNotExistItems();
    }

    public interface IRestoreDestinationFolder
    {
        string Name { get; }
        string Id { get; }
    }

    public class RestoreServerDestinationFolder : IRestoreDestinationFolder
    {
        public RestoreServerDestinationFolder(Folder folder)
        {
            Folder = folder;
        }
        public Folder Folder { get; set; }
        public string Name
        {
            get
            {
                return Folder.DisplayName;
            }
        }
        public string Id
        {
            get
            {
                return Folder.Id.UniqueId;
            }
        }
    }

    public class RestoreLocalDestinationFolder : IRestoreDestinationFolder
    {
        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get; set;
        }
    }
}
