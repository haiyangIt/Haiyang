using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.CompoundFile;

namespace Arcserve.Exchange.FastTransferUtil.Item.PropValue
{
    public class PropList : FTNodeCollection<IPropValue>
    {
        public override bool IsTagRight(PropertyTag propertyTag)
        {
            return PropertyTag.IsProperty(propertyTag);
        }

        protected override IPropValue CreateItem(PropertyTag propertyTag)
        {
            return FTFactory.Instance.CreatePropValue(propertyTag);
        }
    }
}
