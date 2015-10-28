using EwsDataInterface;
using System;
using System.Collections.Generic;

namespace DataProtectInterface.Event
{
    public class CatalogItemArgs : EventArgs
    {
        private IItemData CurrentItem;
        private IFolderData Folder;
        private Stack<IFolderData> FolderStack;
        private IMailboxData Mailbox;
        private CatalogItemProgressType Type;

        public CatalogItemArgs(CatalogItemProgressType type, IMailboxData mailbox, Stack<IFolderData> folderStack, IFolderData folder, IItemData currentItem)
        {
            this.Type = type;
            this.Mailbox = mailbox;
            this.FolderStack = folderStack;
            this.Folder = folder;
            this.CurrentItem = currentItem;
        }
    }
}