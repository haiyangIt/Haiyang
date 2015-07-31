using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item
{
    public class AttachmentCollection : FTNodeCollection<Attachment>
    {
        protected override Attachment CreateItem(PropertyTag propertyTag)
        {
            return FTFactory.Instance.CreateAttachment(propertyTag);
        }

        public override bool IsTagRight(PropertyTag propertyTag)
        {
            return Attachment.IsAttachmentBeginTag(propertyTag);
        }
    }
}
