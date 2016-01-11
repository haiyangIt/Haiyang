using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DataProtectInterface.Util
{
    public class ItemClassUtil
    {
        public static string GetItemClassStr(IItemBase itemBase)
        {
            if (itemBase.ItemKind != ItemKind.Item)
                throw new ArgumentException("Type is wrong.", "itemBase");
            if (itemBase is LoadedTreeItem)
            {
                var str = ((LoadedTreeItem)itemBase).ItemData;
                var des = new JavaScriptSerializer();
                ItemModel item = des.Deserialize<ItemModel>(str);
                return item.ItemClass;
            }
            else if (itemBase is IItemData)
            {
                return ((IItemData)itemBase).ItemClass;
            }
            else
                throw new NotSupportedException("Type is wrong.");
        }

        public static ItemClass GetItemClass(IItemBase itemBase)
        {
            return GetItemClass(GetItemClassStr(itemBase));
        }

        public static string GetItemSuffix(IItemBase itemBase)
        {
            return GetItemSuffix(GetItemClass(itemBase));
        }

        private readonly static Dictionary<string, ItemClass> _itemClassDic = new Dictionary<string, ItemClass>()
        {
            {"IPM.Contact", ItemClass.Contact},
            {"IPM.Appointment", ItemClass.Appointment },
            {"IPM.Note", ItemClass.Message },
            {"IPM.Task", ItemClass.Task }
        };

        private readonly static Dictionary<ItemClass, string> _itemClassSuffixDic = new Dictionary<ItemClass, string>()
        {
            { ItemClass.Contact, ".vcf" },
            { ItemClass.Appointment ,".vcs" },
            { ItemClass.Message,".eml" },
            { ItemClass.Task, ".eml" }
        };

        public static ItemClass GetItemClass(string itemClass)
        {
            ItemClass result;
            if (_itemClassDic.TryGetValue(itemClass, out result))
                return result;

            else if (itemClass.IndexOf("IPM.Appointment") >= 0)
                return ItemClass.Contact;

            else
                throw new NotSupportedException("Modify code to support this type");
        }

        public static string GetItemSuffix(ItemClass itemClass)
        {
            string result;
            if (_itemClassSuffixDic.TryGetValue(itemClass, out result))
                return result;

            else
                throw new NotSupportedException("Modify code to support this type");
        }


        class ItemModel : IItemData
        {
            public int ActualSize
            {
                get; set;
            }

            public DateTime? CreateTime
            {
                get; set;
            }

            public object Data
            {
                get; set;
            }

            public string DisplayName
            {
                get; set;
            }

            public string Id
            {
                get; set;
            }

            public string ItemClass
            {
                get; set;
            }

            public string ItemId
            {
                get; set;
            }

            public ItemKind ItemKind
            {
                get; set;
            }

            public string Location
            {
                get; set;
            }

            public string ParentFolderId
            {
                get; set;
            }

            public int Size
            {
                get; set;
            }

            public IItemData Clone()
            {
                throw new NotImplementedException();
            }
        }
    }

    public enum ItemClass
    {
        None = 0,
        Message = 1,
        Appointment = 2,
        Contact = 3,
        Task = 4
    }

    public class FolderClassUtil
    {
        private static HashSet<string> _validFolderType;

        public static readonly string DefaultFolderType = FolderDataBaseDefault.FolderDefaultType;

        private static HashSet<string> ValidFolderType
        {
            get
            {
                if (_validFolderType == null)
                {
                    _validFolderType = new HashSet<string>();
                    _validFolderType.Add("IPF.Note");
                    _validFolderType.Add("IPF.Appointment");
                    _validFolderType.Add("IPF.Contact");
                    _validFolderType.Add("IPF.Task");
                }
                return _validFolderType;
            }
        }

        public static bool IsFolderValid(string folderClass)
        {
            return ValidFolderType.Contains(folderClass);
        }

        public static IFolderDataBase NewFolderDataBase(IItemBase itemBase)
        {
            switch (itemBase.ItemKind)
            {
                case ItemKind.Folder:
                    return new FolderDataBaseDefault() { DisplayName = itemBase.DisplayName, FolderType = GetFolderType(itemBase) };
                default:
                    return new FolderDataBaseDefault() { DisplayName = itemBase.DisplayName, FolderType = DefaultFolderType };
            }
        }

        public static string GetFolderType(IItemBase itemBase)
        {
            if (itemBase.ItemKind != ItemKind.Folder)
                throw new ArgumentException("Type is wrong.", "itemBase");
            if (itemBase is LoadedTreeItem)
            {
                var str = ((LoadedTreeItem)itemBase).ItemData;
                var des = new JavaScriptSerializer();
                FolderModel item = des.Deserialize<FolderModel>(str);
                return item.FolderType;
            }
            else if (itemBase is IFolderData)
            {
                return ((IFolderData)itemBase).FolderType;
            }
            else
                throw new NotSupportedException("Type is wrong.");
        }

        class FolderModel : IFolderData
        {
            public int ChildFolderCount
            {
                get; set;
            }

            public int ChildItemCount
            {
                get; set;
            }

            public string DisplayName
            {
                get; set;
            }

            public string FolderId
            {
                get; set;
            }

            public string FolderType
            {
                get; set;
            }

            public string Id
            {
                get; set;
            }

            public ItemKind ItemKind
            {
                get; set;
            }

            public string Location
            {
                get; set;
            }

            public string MailboxAddress
            {
                get; set;
            }

            public string ParentFolderId
            {
                get; set;
            }

            public IFolderData Clone()
            {
                throw new NotImplementedException();
            }
        }
    }
}
