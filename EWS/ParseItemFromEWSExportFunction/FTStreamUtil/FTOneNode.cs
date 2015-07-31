using FTStreamUtil.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public abstract class FTOneNode<T> : FTNodeBase, IContentProcess where T : IFTTreeNode
    {
        protected T _item;
        public FTOneNode()
            : base()
        {
            
        }
        protected override void ParseNode()
        {
            var propertyTag = FTStreamParseContext.Instance.ReadPropertyTag();
            if (IsTagRight(propertyTag))
            {
                _item = CreateItem(propertyTag);
                Children.Add(_item);
                _item.Parse();
            }
        }

        protected abstract T CreateItem(PropertyTag propertyTag);

        public abstract bool IsTagRight(PropertyTag propertyTag);
    }
}
