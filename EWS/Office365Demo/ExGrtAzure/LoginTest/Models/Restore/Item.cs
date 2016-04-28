using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Arcserve.Office365.Exchange.Data;

namespace Demo.Models.Restore
{
    public class Item : IItemBase
    {
        public Item()
        {
            CanSelect = 1;
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public object OtherInformation { get; set; }
        public List<Item> Container { get; set; }
        public List<Item> Leaf { get; set; }

        /// <summary>
        /// Container.Count + leaf.Count
        /// </summary>
        public int ChildCount { get; set; }
        public string ItemType { get; set; }

        public int CanSelect { get; set; }

        public ItemKind ItemKind
        {
            get
            {
                return ItemType.GetItemKind();
            }
        }
    }
}