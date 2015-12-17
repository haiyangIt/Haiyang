using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IItemBase
    {
        string Id { get;  }
        string DisplayName { get;  }

        ItemKind ItemKind { get;  }
    }

    public enum ItemKind : byte
    {
        None = 100,
        Organization = 0,
        Mailbox = 1,
        Folder = 2,
        Item = 3
    }

    public class ItemTypeStr
    {
        public const string Organization = "Organization";
        public const string Mailbox = "Mailbox";
        public const string Folder = "Folder";
        public const string Item = "Item";
        public const string None = "None";
    }

    //public class SelectedTreeItemType
    //{
    //    public const string Root = "Root";
    //    public const string Mailbox = "Mailbox";
    //    public const string Folder = "Folder";
    //    public const string Item = "Item";
    //}
}
