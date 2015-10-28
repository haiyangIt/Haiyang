using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IFolderData
    {
        string ParentFolderId { get; }
        string MailboxAddress { get; }
        string Location { get; set; }
        string DisplayName { get; }
        string FolderId { get; }
        string FolderType { get; }
        int ChildItemCount { get; }
        int ChildFolderCount { get; }

        IFolderData Clone();
    }
}
