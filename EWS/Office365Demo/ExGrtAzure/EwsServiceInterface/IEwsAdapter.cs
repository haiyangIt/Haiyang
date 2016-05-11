using EwsDataInterface;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsServiceInterface
{
    public interface IEwsAdapter 
    {
        string ConnectMailbox(EwsServiceArgument argument, string connectMailAddress);

        string MailboxPrincipalAddress { get; }

        //ExchangeService CurrentExchangeService { get; set; }


        List<IFolderData> GetChildFolder(string folderId);

        IDataConvert DataConvert { get; set; }

        IFolderData GetRootFolder();

        bool IsFolderNeedGenerateCatalog(IFolderData folder);
        //IFolderData CreateChildFolder(IFolderDataBase folder, IFolderData parentFolderId);
        //IFolderData FindFolder(IFolderDataBase folder, IFolderData parentFolderId, int findCount = 0);
        void DeleteFolder(string findFolderId, DeleteMode deleteMode = DeleteMode.MoveToDeletedItems);
        string CreateFoldersIfNotExist(List<IFolderDataBase> folderPaths);

        string GetContactFolderId();
        string GetAppointmentFolderId();
        string GetTaskFolderId();

        List<IItemData> GetFolderItems(IFolderData folder);

        void ExportItem(IItemData item, Stream stream, EwsServiceArgument argument);
        void ImportItem(string parentFolderId, byte[] itemData, EwsServiceArgument argument);
        void ImportItem(string parentFolderId, Stream stream, EwsServiceArgument argument);

        bool IsItemNew(IItemData item, DateTime lastTime, DateTime thisTime);

        void ExportEmlItem(IItemData itemInEws, MemoryStream emlStream, EwsServiceArgument argument);

    }
}
