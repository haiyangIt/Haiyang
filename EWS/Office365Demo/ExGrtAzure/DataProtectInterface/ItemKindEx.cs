using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public static class ItemKindEx
    {
        public static Dictionary<ItemKind, string> ItemKind2String = new Dictionary<ItemKind, string>()
        {
            {ItemKind.Organization,  ItemTypeStr.Organization}, {ItemKind.Mailbox, ItemTypeStr.Mailbox }, {ItemKind.Folder, ItemTypeStr.Folder }, {ItemKind.Item, ItemTypeStr.Item }
        };

        public static Dictionary<string, ItemKind > ItemString2Kind = new Dictionary<string, ItemKind>()
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
}
