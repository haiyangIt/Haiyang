using Arcserve.Exchange.FastTransferUtil.FTStream;
using Arcserve.Exchange.FastTransferUtil.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Arcserve.Exchange.FastTransferUtil
{
    public abstract class FTNodeCollection<T> : FTNodeBase, IContentProcess, IEnumerable<IFTTreeNode> where T : IFTTreeNode
    {
        protected override void ParseNode(IFTStreamReader reader)
        {
            ItemCount = 0;
            while (true)
            {
                var propertyTag = reader.ReadPropertyTag();
                if (!IsTagRight(propertyTag))
                    break;
                IFTTreeNode item = CreateItem(propertyTag);
                AddItem(item);
                item.Parse(reader);
                ItemCount++;
            }
        }

        public int ItemCount { get; set; }

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

        public IEnumerator<IFTTreeNode> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Children.GetEnumerator();
        }
    }
}
