using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsServiceInterface
{
    /// <summary>
    /// An interface for getting folder information from EWS.
    /// </summary>
    public interface IFolder
    {
        List<Folder> GetChildFolder(Folder parentFolder);
        Folder GetRootFolder();
        string GetFolderDisplayName(Folder folder);
        ExchangeService CurrentExchangeService { get; }

        bool IsFolderNeedGenerateCatalog(Folder folder);
        FolderId CreateChildFolder(string folderDisplayName, FolderId parentFolderId);
        FolderId FindFolder(string folderDisplayName, FolderId parentFolderId, int findCount = 0);
        void DeleteFolder(FolderId findFolderId, DeleteMode deleteMode = DeleteMode.MoveToDeletedItems);
    }
}
