using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Thread;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Restore
{
    public interface IRestoreToPosition<ProgressType> : ITaskSyncContext<ProgressType>
    {
        IExchangeAccessForRestore<ProgressType> ExchangeAccessForRestore { get; set; }

        OrganizationAdminInfo AdminInfo { get; set; }

        void ConnectExchangeService();
        IRestoreFolder GetAndCreateFolderIfFolderNotExist(IFolderDataSync folder, IRestoreFolder parentFolder);
        void ImportItems(IEnumerable<ImportItemStatus> partition, IRestoreFolder folder);
    }

    public interface IRestoreFolder
    {
        string Name { get; set; }
    }

    public class ImportItemStatus
    {
        public IItemDataSync Item;
        public bool IsExist;
        public ImportItemStatus() { }

        public ImportItemStatus(IItemDataSync item, bool isExist)
        {
            Item = item;
            IsExist = isExist;
        }
    }
}
