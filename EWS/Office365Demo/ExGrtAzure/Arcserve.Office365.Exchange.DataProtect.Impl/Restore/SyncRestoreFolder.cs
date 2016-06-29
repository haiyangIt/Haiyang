using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Restore
{
    public class SyncRestoreFolder : RestoreFolderFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccessForRestore<IJobProgress>
    {
        public ICatalogAccessForRestore<IJobProgress> CatalogAccess
        {
            get; set;
        }

        public IDataFromClientForRestore<IJobProgress> DataFromClient
        {
            get; set;
        }

        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter
        {
            get; set;
        }

        protected override IRestoreFolder GetAndCreateIfFolderNotExistToExchange(IEnumerable<IFolderDataSync> folderHierarchy)
        {
            IRestoreFolder parentFolder = null;
            foreach(var folder in folderHierarchy)
            {
                parentFolder = RestoreToPosition.GetAndCreateFolderIfFolderNotExist(folder, parentFolder);
            }
            return parentFolder;
        }

        protected override ItemList GetFolderItemsFromPlanAndCatalog(IFolderDataSync folder, int offset, int pageCount)
        {
            return DataFromClient.GetFolderItems(folder, offset, pageCount, CatalogAccess.GetFolderItemsFromCatalog);
        }

        protected override RestoreItemsFlowTemplate NewRestoreItemFlow()
        {
            var result = new SyncRetoreItems();
            result.CloneExchangeAccess(this);
            result.CloneSyncContext(this);
            result.RestoreToPosition = RestoreToPosition;
            return result;
        }
    }
}
