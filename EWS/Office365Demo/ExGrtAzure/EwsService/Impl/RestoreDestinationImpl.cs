using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using EwsServiceInterface;
using Microsoft.Exchange.WebServices.Data;
using EwsFrame;

namespace EwsService.Impl
{
    public class RestoreDestinationImpl : IRestoreDestination
    {
        public string DesMailboxAddress { get; set; }
        public string DesFolderDisplayNamePath { get; set; }

        private static char[] _pathSplit = "\\".ToCharArray();
        private string[] _desFolderPathArray;
        private string[] DesFolderPathArray
        {
            get
            {
                if(_desFolderPathArray == null)
                {
                    _desFolderPathArray = DesFolderDisplayNamePath.Split(_pathSplit, StringSplitOptions.RemoveEmptyEntries);
                }
                return _desFolderPathArray;
            }
        }

        public ExportType ExportType
        {
            get
            {
                return ExportType.TransferBin;
            }
        }

        private string CurrentMailbox;
        private ExchangeService CurrentExService;
        private void ConnectMailbox(string mailboxAddress)
        {
            if(CurrentMailbox != mailboxAddress)
            {
                if (CurrentMailbox == null)
                {
                    CurrentMailbox = mailboxAddress;
                    RestoreFactory.Instance.GetServiceContext().CurrentMailbox = CurrentMailbox;
                }

                IMailbox mailboxOper = RestoreFactory.Instance.NewMailboxOperatorImpl();
                IServiceContext context = RestoreFactory.Instance.GetServiceContext();
                mailboxOper.ConnectMailbox(context.Argument, mailboxAddress);
                CurrentExService = mailboxOper.CurrentExchangeService;
                CreatedFolders = new Dictionary<string, FolderId>();
            }
        }

        private Dictionary<string, FolderId> CreatedFolders;
        private FolderId FindAndCreateFolder(string folderDisplayName, FolderId parentFolderId)
        {
            FolderId folderId = null;
            var key = string.Format("{0}-{1}", folderDisplayName, parentFolderId.UniqueId);
            if (CreatedFolders.TryGetValue(key, out folderId))
                return folderId;

            IFolder folderOper = RestoreFactory.Instance.NewFolderOperatorImpl(CurrentExService);
            folderId = folderOper.FindFolder(folderDisplayName, parentFolderId, 3);
            if(folderId == null)
                folderId = folderOper.CreateChildFolder(folderDisplayName, parentFolderId);
            CreatedFolders.Add(key, folderId);
            return folderId;
        }

        private FolderId CreateFoldersIfNotExist(ICollection<string> folderDisplayNames)
        {
            ConnectMailbox(DesMailboxAddress);
            IFolder folderOper = RestoreFactory.Instance.NewFolderOperatorImpl(CurrentExService);
            FolderId parentFolderId = folderOper.GetRootFolder().Id;
            foreach(string folderName in folderDisplayNames)
            {
                parentFolderId = FindAndCreateFolder(folderName, parentFolderId);
            }
            return parentFolderId;
        }

        private List<string> GetFolderPathArray(Stack<IFolderData> folders)
        {
            List<string> path = new List<string>(5);
            if (DesFolderPathArray != null)
                path.AddRange(DesFolderPathArray);
            if(folders != null)
            {
                foreach(IFolderData data in folders)
                {
                    path.Add(data.DisplayName);
                }
            }
            return path;
        }

        private List<string> GetFolderPathArray(List<string> folderPaths)
        {
            List<string> path = new List<string>(5);
            if (DesFolderPathArray != null)
                path.AddRange(DesFolderPathArray);
            path.AddRange(folderPaths);
            return path;
        }

        public void WriteItem(IRestoreItemInformation item, byte[] itemData)
        {
            List<string> path = GetFolderPathArray(item.FolderPathes);
            var folder = CreateFoldersIfNotExist(path);
            IItem itemOperatorImpl = RestoreFactory.Instance.NewItemOperatorImpl(CurrentExService);
            var argument = RestoreFactory.Instance.GetServiceContext().Argument;
            itemOperatorImpl.ImportItem(folder, itemData, argument);
        }

        public void InitOtherInformation(params object[] information)
        {
            DesMailboxAddress = information[0] as string;
            DesFolderDisplayNamePath = information[1] as string;
        }

        public void RestoreComplete(bool success, Exception ex)
        {
        }

        public void Dispose()
        {
        }
    }
}
