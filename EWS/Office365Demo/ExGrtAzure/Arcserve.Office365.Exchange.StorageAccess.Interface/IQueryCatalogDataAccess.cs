using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.StorageAccess.Interface
{
    public interface IQueryCatalogDataAccess : IDataAccess
    {
        /// <summary>
        /// All query is based on current catalogjob(this value)
        /// </summary>
        ICatalogJob CatalogJob { get; set; }
        string Organization { get; set; }
        List<IMailboxData> GetAllMailbox();
        List<IFolderData> GetAllChildFolder(IFolderData parentFolder);
        List<IItemData> GetAllChildItems(IFolderData parentFolder);
        [Obsolete("Please use GetItemContent(string itemId, ExportType type)")]
        IItemData GetItemContent(IItemData itemData);
        IItemData GetItemContent(IItemData itemData, ExportType type);
        List<ICatalogJob> GetAllCatalogJob();

        List<IFolderData> GetAllFoldersInMailboxes(string mailboxAddress);
        List<IItemData> GetAllChildItems(string folderId);
        [Obsolete("Please use GetItemContent(string itemId, ExportType type)")]
        IItemData GetItemContent(string itemId, string displayName);

        IItemData GetItemContent(string itemId, string displayName, ExportType type);

        IItemData GetItem(string itemId);
        List<IItemData> GetChildItems(string folderId, int pageIndex, int pageCount);
        int GetChildItemsCount(string folderId);
        List<ICatalogJob> GetCatalogsInOneDay(DateTime day);
        ICatalogJob GetLatestCatalogJob();
        List<DateTime> GetCatalogDaysInMonth(DateTime startTime);
        List<IMailboxData> GetAllMailbox(List<string> excludeIds);
        List<IFolderData> GetAllChildFolder(string id, List<string> excludefolderIds);
        List<IItemData> GetAllChildItems(string id, List<string> excludeItemIds);
        List<IFolderData> GetAllChildFolder(string rootFolderId);
    }
}
