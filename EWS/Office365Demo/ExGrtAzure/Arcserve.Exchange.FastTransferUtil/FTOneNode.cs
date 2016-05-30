using Arcserve.Exchange.FastTransferUtil.FTStream;
using Arcserve.Exchange.FastTransferUtil.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil
{
    public abstract class FTOneNode<T> : FTNodeBase, IContentProcess where T : IFTTreeNode
    {
        protected T _item;
        public FTOneNode()
            : base()
        {
            
        }
        protected override void ParseNode(IFTStreamReader reader)
        {
            var propertyTag = reader.ReadPropertyTag();
            if (IsTagRight(propertyTag))
            {
                _item = CreateItem(propertyTag);
                Children.Add(_item);
                _item.Parse(reader);
            }
        }

        protected abstract T CreateItem(PropertyTag propertyTag);

        public abstract bool IsTagRight(PropertyTag propertyTag);
    }
}
