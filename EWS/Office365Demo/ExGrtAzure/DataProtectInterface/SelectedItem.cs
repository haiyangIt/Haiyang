using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DataProtectInterface
{
    public class LoadedTreeItem : IItemBase
    {
        public string Id { get; set; }
        public int Status { get; set; }
        public int UnloadedChildrenStatus { get; set; }
        public int TotalChildCount { get; set; }
        public int LoadedChildrenCount { get; set; }
        public string ItemType { get; set; }
        public string DisplayName { get; set; }
        public int SelectedChildCount { get; set; }
        public List<LoadedTreeItem> LoadedChildren { get; set; }
        public string ItemData { get; set; }

        public ItemKind ItemKind
        {
            get
            {
                return ItemType.GetItemKind();
            }
        }
    }

    

    public enum SelectedItemStatus : byte
    {
        UnSelected = 0,
        Selected = 1,
        Indeterminate = 2
    }
    
}