using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data
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

    public static class ItemKindEx
    {
        public static Dictionary<ItemKind, string> ItemKind2String = new Dictionary<ItemKind, string>()
        {
            {ItemKind.Organization,  ItemTypeStr.Organization}, {ItemKind.Mailbox, ItemTypeStr.Mailbox }, {ItemKind.Folder, ItemTypeStr.Folder }, {ItemKind.Item, ItemTypeStr.Item }
        };

        public static Dictionary<string, ItemKind> ItemString2Kind = new Dictionary<string, ItemKind>()
        {
            {ItemTypeStr.Organization, ItemKind.Organization}, { ItemTypeStr.Mailbox, ItemKind.Mailbox}, {ItemTypeStr.Folder ,ItemKind.Folder}, {ItemTypeStr.Item, ItemKind.Item}
        };

        public static string GetItemKind(this ItemKind itemKind)
        {
            return ItemKind2String[itemKind];
        }

        public static ItemKind GetItemKind(this string ItemKindStr)
        {
            if (string.IsNullOrEmpty(ItemKindStr))
                return ItemKind.None;
            return ItemString2Kind[ItemKindStr];
        }
    }

    //public class SelectedTreeItemType
    //{
    //    public const string Root = "Root";
    //    public const string Mailbox = "Mailbox";
    //    public const string Folder = "Folder";
    //    public const string Item = "Item";
    //}
}
