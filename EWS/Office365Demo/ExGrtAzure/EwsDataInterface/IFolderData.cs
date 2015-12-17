using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IFolderData : IItemBase
    {
        string ParentFolderId { get; }
        string MailboxAddress { get; }
        string Location { get; set; }
        string FolderId { get; }
        string FolderType { get; }
        int ChildItemCount { get; set; }
        int ChildFolderCount { get; set; }

        IFolderData Clone();
    }
}
