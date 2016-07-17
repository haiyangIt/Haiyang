using Arcserve.Office365.Exchange.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Mail
{
    public static class ItemClassUtil
    {
        public static string GetItemClassStr(IItemBase itemBase)
        {
            if (itemBase.ItemKind != ItemKind.Item)
                throw new ArgumentException("Type is wrong.", "itemBase");
            //if (itemBase is LoadedTreeItem)
            //{
            //    var str = ((LoadedTreeItem)itemBase).ItemData;

            //    ItemModel item = JsonConvert.DeserializeObject<ItemModel>(str);
            //    return item.ItemClass;
            //}
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

        public static string GetItemSuffix(IItemBase itemBase, ExportType exportType)
        {
            if (exportType == ExportType.Msg)
                return ".msg";
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

        public static ItemClass GetItemClass(this string itemClass)
        {
            ItemClass result;
            if (_itemClassDic.TryGetValue(itemClass, out result))
                return result;

            else if (itemClass.IndexOf("IPM.Appointment") >= 0)
                return ItemClass.Appointment;

            else if (itemClass.IndexOf("IPM.Note") >= 0 )
            {
                Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.WARN, string.Format("treat this type {0} as a IPM.Note, please analysis the type, and modify code to support this item type:{0}", itemClass));
                return ItemClass.Message;
            }
            else if(itemClass.IndexOf("IPM.Schedule") >= 0)
            {
                Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.WARN, string.Format("treat this type {0} as a IPM.Appointment, please analysis the type, and modify code to support this item type:{0}", itemClass));
                return ItemClass.Appointment;
            }
            else if(itemClass.IndexOf("IPM.TASK") >= 0)
            {
                Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.WARN, string.Format("treat this type {0} as a IPM.Task, please analysis the type, and modify code to support this item type:{0}", itemClass));
                return ItemClass.Task;
            }
            else
            {
                Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.WARN, string.Format("treat this type {0} as a IPM.None, please analysis the type, and modify code to support this item type:{0}", itemClass));
                return ItemClass.None;
            }
        }

        public static string GetItemSuffix(this ItemClass itemClass)
        {
            string result;
            if (_itemClassSuffixDic.TryGetValue(itemClass, out result))
                return result;

            else
                throw new NotSupportedException(string.Format("Modify code to support this item type:{0}", itemClass));
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


    public static class FolderClassUtil
    {
        static FolderClassUtil()
        {
            ValidFolderType = new HashSet<string>();
            ValidFolderType.Add("IPF.Note");
            ValidFolderType.Add("IPF.Appointment");
            ValidFolderType.Add("IPF.Contact");
            ValidFolderType.Add("IPF.Task");
        }

        public static readonly string DefaultFolderType = FolderDataBaseDefault.FolderDefaultType;

        private static readonly HashSet<string> ValidFolderType;

        private readonly static Dictionary<string, FolderClass> _folderClassDic = new Dictionary<string, FolderClass>()
        {
            {"IPF.Contact", FolderClass.Contact},
            {"IPF.Appointment", FolderClass.Calendar },
            {"IPF.Note", FolderClass.Message },
            {"IPF.Task", FolderClass.Task }
        };

        private readonly static Dictionary<FolderClass, string> _folderClassToStr = new Dictionary<FolderClass, string>()
        {
            { FolderClass.Contact, "IPF.Contact"},
            {FolderClass.Calendar ,"IPF.Appointment"},
            {FolderClass.Message, "IPF.Note" },
            {FolderClass.Task , "IPF.Task"}
        };

        public static FolderClass GetFolderClass(this string folderClass)
        {
            FolderClass result;
            if (_folderClassDic.TryGetValue(folderClass, out result))
                return result;

            else
                throw new NotSupportedException(string.Format("Modify code to support this folder type:{0}", folderClass));
        }

        public static string GetFolderClass(this FolderClass folderClass)
        {
            string result;
            if (_folderClassToStr.TryGetValue(folderClass, out result))
                return result;

            else
                throw new NotSupportedException(string.Format("Modify code to support this folder type:{0}", folderClass));

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
            //if (itemBase is LoadedTreeItem)
            //{
            //    var str = ((LoadedTreeItem)itemBase).ItemData;
            //    FolderModel item = JsonConvert.DeserializeObject<FolderModel>(str);
            //    return item.FolderType;
            //}
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

    public class MailClassUtil
    {
        public static IMailboxData GetMailboxData(IItemBase itemBase)
        {
            if (itemBase.ItemKind != ItemKind.Mailbox)
                throw new ArgumentException("Type is wrong.", "itemBase");
            //if (itemBase is LoadedTreeItem)
            //{
            //    var str = ((LoadedTreeItem)itemBase).ItemData;
            //    MailClass item = JsonConvert.DeserializeObject<MailClass>(str);
            //    return item;
            //}
            else if (itemBase is IMailboxData)
            {
                return itemBase as IMailboxData;
            }
            else
                throw new NotSupportedException(string.Format("Type {0} is wrong.", itemBase.GetType().FullName));
        }

        class MailClass : IMailboxData
        {
            public int ChildFolderCount
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

            public ItemKind ItemKind
            {
                get; set;
            }

            public string Location
            {
                get; set;
            }

            public string MailAddress
            {
                get; set;
            }

            public string RootFolderId
            {
                get; set;
            }

            public IMailboxData Clone()
            {
                throw new NotImplementedException();
            }
        }
    }
}
