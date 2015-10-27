using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IQueryCatalogDataAccess : IDataAccess
    {
        /// <summary>
        /// All query is based on current catalogjob(this value)
        /// </summary>
        ICatalogJob CatalogJob { get; set; }
        List<IMailboxData> GetAllMailbox();
        List<IFolderData> GetAllChildFolder(IFolderData parentFolder);
        List<IItemData> GetAllChildItems(IFolderData parentFolder);
        IItemData GetItemContent(IItemData itemData);
        List<ICatalogJob> GetAllCatalogJob();

        List<IFolderData> GetAllFoldersInMailboxes(string mailboxAddress);
        List<IItemData> GetAllChildItems(string folderId);
        IItemData GetItemContent(string itemId);
        IItemData GetItem(string itemId);
    }
}
