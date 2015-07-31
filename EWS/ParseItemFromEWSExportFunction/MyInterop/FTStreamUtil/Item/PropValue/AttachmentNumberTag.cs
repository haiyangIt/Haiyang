using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public class AttachmentNumberTag : PropValue.MetaPropertyBase
    {
        public const UInt32 AttachmentNumberTagValue = 0x0E210003;
        public override bool IsTagRight(PropertyTag propertyTag)
        {
            if (propertyTag.Data != AttachmentNumberTagValue)
                throw new ArgumentException(string.Format("AttachmentNumber tag data is wrong:{0}.", propertyTag.Data.ToString("X8")));
            return true;
        }
    }
}
