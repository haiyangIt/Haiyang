using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;

namespace DataProtectImpl
{
    public class FilterBySelectedTree : IFilterItemWithMailbox
    {
        private LoadedTreeItem _orgSelectedItems;
        private LoadedTreeItemUtil _orgSelectedItemUtils;
        public FilterBySelectedTree(LoadedTreeItem orgSelectItems)
        {
            _orgSelectedItems = orgSelectItems;
            _orgSelectedItemUtils = new LoadedTreeItemUtil(orgSelectItems);
        }

        public List<IMailboxData> GetAllMailbox()
        {
            List<IMailboxData> mailboxDatas = new List<IMailboxData>(_orgSelectedItems.LoadedChildren.Count);
            foreach(var child in _orgSelectedItems.LoadedChildren)
            {
                var item = new FilterMailbox(child.DisplayName, child.Id);
                mailboxDatas.Add(item);
            }
            return mailboxDatas;
        }

        public bool IsFilterFolder(IFolderData currentFolder, IMailboxData mailbox, Stack<IFolderData> folders)
        {
            var mailboxItem = _orgSelectedItemUtils.GetChild(mailbox.Id);
            if (mailboxItem == null)
            {
                mailboxItem = _orgSelectedItemUtils.GetChildByDisplayName(mailbox.DisplayName);
                if (mailboxItem == null)
                    return true;
            }

            if (mailboxItem.Item.Status == (int)SelectedItemStatus.Selected)
                return false;
            else if (mailboxItem.Item.Status == (int)SelectedItemStatus.UnSelected)
                return true;
            else
            {
                List<IFolderData> list = new List<IFolderData>(folders.Count + 1);
                list.Add(currentFolder);
                list.AddRange(folders);
                Stack<IFolderData> stack = new Stack<IFolderData>(list);
                
                int index = 0;
                LoadedTreeItemUtil folderItemUtil = mailboxItem;
                foreach (var folder in stack)
                {
                    if (index++ == 0) // skip the rootfolder.
                        continue;

                    folderItemUtil = folderItemUtil.GetChild(folder.Id);
                    if (folderItemUtil == null)
                    {
                        folderItemUtil = folderItemUtil.GetChildByDisplayName(folder.DisplayName);
                    }

                    if (folderItemUtil == null)
                        return true;

                    if (folderItemUtil.Item.Status == (int)SelectedItemStatus.Selected)
                        return false;
                    else if (folderItemUtil.Item.Status == (int)SelectedItemStatus.UnSelected)
                        return true;
                    else
                        continue;
                }


            }
            return true;
            
        }

        public bool IsFilterItem(IItemData item, IMailboxData mailbox, Stack<IFolderData> folders)
        {
            return false;
        }

        public bool IsFilterMailbox(IMailboxData mailbox)
        {
            var id = mailbox.Id;
            if (string.IsNullOrEmpty(id))
            {
                id = mailbox.MailAddress;
            }
            var mailboxItem = _orgSelectedItemUtils.GetChild(id);
            if (mailboxItem == null)
            {
                mailboxItem = _orgSelectedItemUtils.GetChildByDisplayName(mailbox.DisplayName);
                if (mailboxItem == null)
                    return true;
            }

            if (mailboxItem.Item.Status == (int)SelectedItemStatus.Selected)
                return false;
            else if (mailboxItem.Item.Status == (int)SelectedItemStatus.UnSelected)
                return true;
            return false;
        }
    }

    class LoadedTreeItemUtil
    {
        public LoadedTreeItem Item;
        Dictionary<string, LoadedTreeItemUtil> Id2Children;
        Dictionary<string, LoadedTreeItemUtil> Display2Children;
        public LoadedTreeItemUtil(LoadedTreeItem item)
        {
            Item = item;

            List<LoadedTreeItemUtil> children = CreateChildren(item.LoadedChildren);
            Id2Children = new Dictionary<string, LoadedTreeItemUtil>(children.Count);
            Display2Children = new Dictionary<string, LoadedTreeItemUtil>(children.Count);

            foreach(var child in children)
            {
                Id2Children[child.Item.Id] = child;
                Display2Children[child.Item.DisplayName] = child;
            }
        }

        private List<LoadedTreeItemUtil> CreateChildren(List<LoadedTreeItem> loadedChildren)
        {
            if(loadedChildren != null)
            {
                var result = new List<LoadedTreeItemUtil>(loadedChildren.Count);
                foreach(var child in loadedChildren)
                {
                    result.Add(new LoadedTreeItemUtil(child));
                }
                return result;
            }
            return new List<LoadedTreeItemUtil>(0);
        }

        public LoadedTreeItemUtil GetChild(string id)
        {
            LoadedTreeItemUtil result = null;
            if(Id2Children.TryGetValue(id, out result))
            {
                return result;
            }
            return null;
        }

        public LoadedTreeItemUtil GetChildByDisplayName(string displayName)
        {
            LoadedTreeItemUtil result = null;
            if (Display2Children.TryGetValue(displayName, out result))
            {
                return result;
            }
            return null;
        }
    }

    class FilterMailbox : IMailboxData
    {

        public FilterMailbox(string displayName, string mailboxAddress)
        {
            DisplayName = displayName;
            MailAddress = mailboxAddress;
        }

        public int ChildFolderCount
        {
            get; set;
        }

        public string DisplayName
        {
            get; set;
        }

        public string Id
        {
            get
            {
                return RootFolderId;
            }

            set
            {
                RootFolderId = value;
            }
        }

        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Mailbox;
            }

            set
            {
            }
        }

        public string Location
        {
            get; set;
        }

        public string MailAddress
        {
            get; private set;
        }

        public string RootFolderId
        {
            get; set;
        }

        public IMailboxData Clone()
        {
            return new FilterMailbox(DisplayName, MailAddress)
            {
                Location = Location,
                RootFolderId = RootFolderId
            };
        }
    }
}
