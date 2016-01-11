using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using EwsFrame;

namespace DataProtectImpl
{
    public class RestoreServiceEx : IRestoreServiceEx
    {
        public RestoreServiceEx(string adminUserName, string adminPassword, string domainName, string organizationName)
        {
            if (string.IsNullOrEmpty(adminUserName))
                throw new ArgumentException("user name is null", "adminUserName");
            if (string.IsNullOrEmpty(adminPassword))
                throw new ArgumentException("password is null", "adminPassword");

            AdminInfo = new OrganizationAdminInfo();
            AdminInfo.UserName = adminUserName;
            AdminInfo.UserPassword = adminPassword;
            AdminInfo.UserDomain = domainName;
            AdminInfo.OrganizationName = organizationName;

            ServiceContext = EwsFrame.ServiceContext.NewServiceContext(AdminInfo.UserName, AdminInfo.UserPassword, AdminInfo.UserDomain, AdminInfo.OrganizationName, TaskType.Restore);
        }

        public OrganizationAdminInfo AdminInfo
        {
            get; set;
        }

        public ICatalogJob CurrentRestoreCatalogJob
        {
            get; set;
        }

        public IRestoreDestinationEx Destination
        {
            get; set;
        }

        private string _restoreJobName;
        public virtual string RestoreJobName
        {
            get
            {
                if (string.IsNullOrEmpty(_restoreJobName))
                {
                    _restoreJobName = string.Format("{0} Restore Job {1}", AdminInfo.OrganizationName, StartTime.ToString("yyyyMMddHHmmss"));
                }
                return _restoreJobName;
            }
            set
            {
                _restoreJobName = value;
            }
        }

        public IServiceContext ServiceContext
        {
            get; private set;
        }

        public DateTime StartTime
        {
            get; private set;
        }

        private IQueryCatalogDataAccess DataAccess
        {
            get
            {
                var catalogQueryDataAccess = (IQueryCatalogDataAccess)ServiceContext.DataAccessObj;
                catalogQueryDataAccess.CatalogJob = CurrentRestoreCatalogJob;
                return catalogQueryDataAccess;
            }
        }

        public void Restore(LoadedTreeItem item)
        {
            RestoreStart();
            item.DisplayName = CurrentRestoreCatalogJob.Organization;
            Stack<IItemBase> stacks = new Stack<IItemBase>(8);
            RestoreSelectItem(item, Destination, stacks);
            RestoreEnd();
        }

        private void CheckLoadedTreeItem(LoadedTreeItem item, out bool isLoadUnloadChildren, out bool isEnd)
        {
            var itemStatus = (SelectedItemStatus)item.Status;
            var isLoadedAllChild = item.LoadedChildrenCount == item.TotalChildCount;
            var isAllLoadedChildSelected = item.LoadedChildrenCount == item.SelectedChildCount;
            var isNoLoadedChildSelected = item.SelectedChildCount == 0;
            var itemUnloadStatus = (SelectedItemStatus)item.UnloadedChildrenStatus;

            if (itemUnloadStatus == SelectedItemStatus.Indeterminate)
                throw new ArgumentException("unload status must not be indeterminate.");

            isLoadUnloadChildren = false;
            isEnd = false;
            switch (itemStatus)
            {
                case SelectedItemStatus.Selected:

                    // loaded
                    if (!isAllLoadedChildSelected)
                        throw new ArgumentException("when item status is selected status, all loaded item need select. please check code.");

                    // unloaded
                    else if (!isLoadedAllChild && itemUnloadStatus != SelectedItemStatus.Selected)
                        throw new ArgumentException("when item status is selected status, unloaded status must be selected. please check code.");

                    else if (isLoadedAllChild)
                    {
                        // all load select items.
                    }
                    else
                    {
                        isLoadUnloadChildren = true;
                        // load selected item + unload items.
                    }
                    break;
                case SelectedItemStatus.Indeterminate:

                    if (isLoadedAllChild)
                    {
                        if (isAllLoadedChildSelected)
                        {
                            throw new ArgumentException("when item status is indeterminate and all child is loaded, only part of items select.");
                        }

                        // all load select items.
                    }
                    else
                    {
                        if (itemUnloadStatus == SelectedItemStatus.UnSelected)
                            isLoadUnloadChildren = false;
                        else if (itemUnloadStatus == SelectedItemStatus.Selected)
                            isLoadUnloadChildren = true;
                        else
                            throw new ArgumentException("when item status is indeterminate and part child is loaded, the unload status must be selected.");
                        
                    }
                    break;
                case SelectedItemStatus.UnSelected:
                    if (!isNoLoadedChildSelected)
                        throw new ArgumentException("when item status is unselected, all loaded item must not select.");
                    else if (itemUnloadStatus != SelectedItemStatus.UnSelected)
                        throw new ArgumentException("when item status is unselected, all unloaded item must not select.");
                    else
                        isEnd = true;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Don't support the selecte statuls {0}", item.Status));
            }

        }

        private void GetUnloadedChild(LoadedTreeItem item, out List<IMailboxData> childMailboxes, out List<IFolderData> childFolders, out List<IItemData> childMailItems)
        {
            childMailboxes = null;
            childFolders = null;
            childMailItems = null;

            List<string> loadedMailChildrenIds = new List<string>();
            List<string> loadedFolderChildrenIds = new List<string>();
            List<string> loadedItemChildrenIds = new List<string>();

            if(item.LoadedChildrenCount > 0)
                foreach (var loadChildItem in item.LoadedChildren)
                {
                    switch (loadChildItem.ItemKind)
                    {
                        case ItemKind.Organization:
                            throw new NotSupportedException("no this type child.");
                        case ItemKind.Mailbox:
                            loadedMailChildrenIds.Add(loadChildItem.Id);
                            break;
                        case ItemKind.Folder:
                            loadedFolderChildrenIds.Add(loadChildItem.Id);
                            break;
                        case ItemKind.Item:
                            loadedItemChildrenIds.Add(loadChildItem.Id);
                            break;
                        default:
                            throw new NotSupportedException(string.Format("Don't support the type {0}", item.ItemType));
                    }
                }

            switch (item.ItemKind)
            {
                case ItemKind.Organization:
                    childMailboxes = DataAccess.GetAllMailbox(loadedMailChildrenIds);
                    break;
                case ItemKind.Mailbox:
                    childFolders = DataAccess.GetAllChildFolder(item.Id, loadedFolderChildrenIds);
                    break;
                case ItemKind.Folder:
                    childFolders = DataAccess.GetAllChildFolder(item.Id, loadedFolderChildrenIds);
                    childMailItems = DataAccess.GetAllChildItems(item.Id, loadedItemChildrenIds);
                    break;
                case ItemKind.Item:
                    throw new NotSupportedException("unload mail items need deal with in unload folder.");
                default:
                    throw new NotSupportedException(string.Format("Don't support the type {0}", item.ItemType));
            }


        }

        public void RestoreSelectItem(LoadedTreeItem item, IRestoreDestinationEx restoreDestination, Stack<IItemBase> dealItemStack)
        {
            bool isEnd = false;
            bool isLoadUnloadChildren = false;

            CheckLoadedTreeItem(item, out isLoadUnloadChildren, out isEnd);
            if (isEnd)
                return;

            List<IMailboxData> childMailboxes = null;
            List<IFolderData> childFolders = null;
            List<IItemData> childMailItems = null;
            if (isLoadUnloadChildren)
            {
                GetUnloadedChild(item, out childMailboxes, out childFolders, out childMailItems);
            }

            dealItemStack.Push(item);
            // deal self.
            if (item.Status == (byte)SelectedItemStatus.Selected)
            {
                switch (item.ItemKind)
                {
                    case ItemKind.Organization:
                        RestoreOrganizationToDestination(CurrentRestoreCatalogJob.Organization, restoreDestination, dealItemStack);
                        break;
                    case ItemKind.Mailbox:
                        RestoreMailboxToDestination(item.DisplayName, restoreDestination, dealItemStack);
                        break;
                    case ItemKind.Folder:
                        RestoreFolderToDestination(item.DisplayName, restoreDestination, dealItemStack);
                        break;
                    case ItemKind.Item:
                        RestoreItemToDestination(item.Id, item.DisplayName, restoreDestination, dealItemStack);
                        break;
                }
            }

            // deal loaded children.
            if (item.LoadedChildren != null)
            {
                foreach (var loadChildItem in item.LoadedChildren)
                    RestoreSelectItem(loadChildItem, restoreDestination, dealItemStack);
            }

            // deal unloaded children.
            if (isLoadUnloadChildren)
                RestoreUnloadedChildren(item, childMailboxes, childFolders, childMailItems, restoreDestination, dealItemStack);

            dealItemStack.Pop();
        }

        private void RestoreUnloadedChildren(LoadedTreeItem item,
            List<IMailboxData> childMailboxes,
            List<IFolderData> childFolders,
            List<IItemData> childMailItems,
            IRestoreDestinationEx restoreDestination,
            Stack<IItemBase> dealItemStack)
        {
            switch (item.ItemKind)
            {
                case ItemKind.Organization:
                    RestoreUnLoadedMailbox(childMailboxes, restoreDestination, dealItemStack);
                    break;
                case ItemKind.Mailbox:
                    RestoreUnLoadedFolders(childFolders, restoreDestination, dealItemStack);
                    break;
                case ItemKind.Folder:
                    RestoreUnLoadedFolders(childFolders, restoreDestination, dealItemStack);
                    RestoreUnLoadedItems(childMailItems, restoreDestination, dealItemStack);
                    break;
                case ItemKind.Item:
                    throw new ArgumentException();
                default:
                    throw new NotSupportedException();
            }
        }

        private void RestoreUnLoadedItems(List<IItemData> childMailItems, IRestoreDestinationEx restoreDestination, Stack<IItemBase> dealItemStack)
        {
            foreach (var item in childMailItems)
            {
                dealItemStack.Push(item);
                RestoreItemToDestination(item.ItemId, item.DisplayName, restoreDestination, dealItemStack);
                dealItemStack.Pop();
            }
        }

        private void RestoreUnLoadedFolders(List<IFolderData> folders, IRestoreDestinationEx restoreDestination, Stack<IItemBase> dealItemStack)
        {
            foreach (var folder in folders)
            {
                dealItemStack.Push(folder);
                RestoreFolderToDestination(((IItemBase)folder).DisplayName, restoreDestination, dealItemStack);

                var childFolders = DataAccess.GetAllChildFolder(folder);
                RestoreUnLoadedFolders(childFolders, restoreDestination, dealItemStack);

                var childItems = DataAccess.GetAllChildItems(folder);
                RestoreUnLoadedItems(childItems, restoreDestination, dealItemStack);
                dealItemStack.Pop();
            }
        }

        private void RestoreUnLoadedMailbox(List<IMailboxData> mailboxes, IRestoreDestinationEx restoreDestination, Stack<IItemBase> dealItemStack)
        {
            foreach (var mailbox in mailboxes)
            {
                dealItemStack.Push(mailbox);
                RestoreMailboxToDestination(mailbox.DisplayName, restoreDestination, dealItemStack);

                var childFolders = DataAccess.GetAllChildFolder(mailbox.RootFolderId);
                RestoreUnLoadedFolders(childFolders, restoreDestination, dealItemStack);
                dealItemStack.Pop();
            }
        }

        private void RestoreItemToDestination(string id, string displayName, IRestoreDestinationEx restoreDestination, Stack<IItemBase> dealItemStack)
        {
            IItemData itemDetails = DataAccess.GetItemContent(id, displayName, restoreDestination.ExportType);
            restoreDestination.DealItem(id, displayName, itemDetails.Data as byte[], dealItemStack);
        }

        private void RestoreFolderToDestination(string displayName, IRestoreDestinationEx restoreDestination, Stack<IItemBase> dealItemStack)
        {
            restoreDestination.DealFolder(displayName, dealItemStack);
        }

        private void RestoreMailboxToDestination(string displayName, IRestoreDestinationEx restoreDestination, Stack<IItemBase> dealItemStack)
        {
            restoreDestination.DealMailbox(displayName, dealItemStack);
        }

        private void RestoreOrganizationToDestination(string organization, IRestoreDestinationEx restoreDestination, Stack<IItemBase> dealItemStack)
        {
            restoreDestination.DealOrganization(organization, dealItemStack);
        }

        private void RestoreEnd()
        {
            Destination.RestoreComplete(true, null);
        }

        private void RestoreStart()
        {
            StartTime = DateTime.Now;
        }
    }
}
