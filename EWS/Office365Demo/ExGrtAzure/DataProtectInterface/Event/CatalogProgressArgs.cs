using EwsDataInterface;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace DataProtectInterface.Event
{
    public class CatalogProgressArgs : EventArgs
    {
        public DateTime Time = DateTime.UtcNow;
        public IMailboxData Mailbox;
        public Process MailboxProcess;
        public Process FolderProcess;
        public Process ItemProcess;
        public IFolderData CurrentFolder;
        public IItemData CurrentItem;
        public CatalogProgressType Type;
        public CatalogMailboxProgressType MailboxProgressType;
        public CatalogFolderProgressType FolderProgressType;
        public CatalogItemProgressType ItemProgressType;

        public CatalogProgressArgs(CatalogProgressType type)
        {
            Type = type;
        }

        public CatalogProgressArgs(CatalogMailboxProgressType mailboxProgressType, Process mailboxProcess, IMailboxData mailbox)
        {
            Type = CatalogProgressType.GRTForMailboxRunning;
            MailboxProgressType = mailboxProgressType;
            this.MailboxProcess = mailboxProcess;
            this.Mailbox = mailbox;
        }

        public CatalogProgressArgs(CatalogFolderProgressType type, Process mailboxProcess, IMailboxData mailbox, Process folderProcess, IFolderData currentFolder)
        {
            Type = CatalogProgressType.GRTForFolderRunning;
            FolderProgressType = type;
            this.MailboxProcess = mailboxProcess;
            this.Mailbox = mailbox;
            this.CurrentFolder = currentFolder;
            this.FolderProcess = folderProcess;
        }


        public CatalogProgressArgs(CatalogItemProgressType type, Process mailboxProcess, IMailboxData mailbox, 
            Process folderProcess, IFolderData currentFolder,
            Process itemProcess, IItemData currentItem)
        {
            Type = CatalogProgressType.GRTForItemRunning;
            ItemProgressType = type;
            this.MailboxProcess = mailboxProcess;
            this.Mailbox = mailbox;
            this.CurrentFolder = currentFolder;
            this.FolderProcess = folderProcess;
            this.CurrentItem = currentItem;
            this.ItemProcess = itemProcess;
        }

        public string GetUIString()
        {
            return GetString(this); // todo.
        }

        public static string GetString(CatalogProgressArgs progress)
        {
            return JsonConvert.SerializeObject(progress);
        }

        public static CatalogProgressArgs StringToObj(string progressStr)
        {
            return JsonConvert.DeserializeObject<CatalogProgressArgs>(progressStr);
        }
    }
}