using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.CompoundFile;

namespace Arcserve.Exchange.FastTransferUtil.Item.PropValue
{
    public abstract class MetaPropertyBase : FTOneNode<IPropValue> , IFTTransferUnit
    {
        protected override IPropValue CreateItem(PropertyTag propertyTag)
        {
            return FTFactory.Instance.CreatePropValue(propertyTag);
        }

        internal void InitItem(IPropValue propValue)
        {
            _item = propValue;
            Children.Add(_item);
        }

        public override void WriteToCompoundFile(CompoundFileBuild build)
        {
            
        }
    }
}
