using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastTransferUtil.CompoundFile;

namespace FTStreamUtil.Item.PropValue
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
