using FTStreamUtil.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public abstract class FTNodeCollection<T> : FTNodeBase, IContentProcess where T : IFTTreeNode
    {
        protected override void ParseNode()
        {
            while (true)
            {
                var propertyTag = FTStreamParseContext.Instance.ReadPropertyTag();
                if (!IsTagRight(propertyTag))
                    break;
                IFTTreeNode item = CreateItem(propertyTag);
                AddItem(item);
                item.Parse();
            }
        }

        internal void AddItem(IFTTreeNode item)
        {
            Children.Add(item);
        }

        internal T AddItem()
        {
            T item = CreateItem(null);
            Children.Add(item);
            return item;
        }

        protected abstract T CreateItem(PropertyTag propertyTag);

        public abstract bool IsTagRight(PropertyTag propertyTag);
    }
}
