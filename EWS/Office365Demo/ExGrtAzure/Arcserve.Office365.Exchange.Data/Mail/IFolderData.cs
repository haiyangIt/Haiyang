using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Mail
{
    public interface IFolderData : IFolderDataBase, IItemBase
    {
        string ParentFolderId { get; }
        string MailboxAddress { get; }
        string Location { get; set; }
        int ChildItemCount { get; set; }
        int ChildFolderCount { get; set; }
        string FolderId { get; }
        IFolderData Clone();
    }

    public interface IFolderDataBase
    {
        string DisplayName { get; }
        string FolderType { get; }
    }

    public class FolderDataBaseDefault : IFolderDataBase
    {
        public readonly static string FolderDefaultType = "IPF.Note";
        public FolderDataBaseDefault()
        {
            FolderType = FolderDefaultType;
        }

        public string DisplayName
        {
            get; set;
        }

        public string FolderType
        {
            get; set;
        }
    }

}
