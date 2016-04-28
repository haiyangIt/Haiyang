using Arcserve.Office365.Exchange.Data.Mail;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi
{
    /// <summary>
    /// An interface for getting folder information from EWS.
    /// </summary>
    public interface IFolder
    {
        List<Folder> GetChildFolder(Folder parentFolder);

        List<Folder> GetChildFolder(string folderId);

        Folder GetRootFolder();

        string GetFolderDisplayName(Folder folder);
        ExchangeService CurrentExchangeService { get; }

        bool IsFolderNeedGenerateCatalog(Folder folder);
        FolderId CreateChildFolder(IFolderDataBase folder, FolderId parentFolderId);
        FolderId FindFolder(IFolderDataBase folder, FolderId parentFolderId, int findCount = 0);
        void DeleteFolder(FolderId findFolderId, DeleteMode deleteMode = DeleteMode.MoveToDeletedItems);
    }

}
