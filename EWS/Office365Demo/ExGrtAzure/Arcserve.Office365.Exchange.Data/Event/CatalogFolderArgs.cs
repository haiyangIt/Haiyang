using Arcserve.Office365.Exchange.Data.Mail;
using System;
using System.Collections.Generic;

namespace Arcserve.Office365.Exchange.Data.Event
{
    public class CatalogFolderArgs : EventArgs
    {
        private Process ChildFolderProcess;
        private IFolderData CurrentFolder;
        private IItemData CurrentItem;
        private IMailboxData CurrentMailbox;
        private Stack<IFolderData> FolderStack;
        private Process ItemProcess;
        private CatalogFolderProgressType Type;

        public CatalogFolderArgs(CatalogFolderProgressType type, IMailboxData currentMailbox, Stack<IFolderData> folderStack, IFolderData currentFolder, Process itemProcess, IItemData currentItem, Process childFolderProcess)
        {
            this.Type = type;
            this.CurrentMailbox = currentMailbox;
            this.FolderStack = folderStack;
            this.CurrentFolder = currentFolder;
            this.ItemProcess = itemProcess;
            this.CurrentItem = currentItem;
            this.ChildFolderProcess = childFolderProcess;
        }
    }
}