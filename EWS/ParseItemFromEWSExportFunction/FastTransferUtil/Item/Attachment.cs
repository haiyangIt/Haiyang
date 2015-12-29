using FastTransferUtil.CompoundFile;
using FTStreamUtil.Item.Marker;
using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item
{
    public class Attachment : FTNodeBase
    {

        public Attachment()
            : base()
        {
            NewAttachment = FTFactory.Instance.CreateNewAttachmentMarker();
            AttachmentNumber = FTFactory.Instance.CreateAttachmentNumber();
            AttachmentContent = FTFactory.Instance.CreateAttachmentContent();
            EndAttachment = FTFactory.Instance.CreateEndAttachmentMarker();

            Children.Add(NewAttachment);
            Children.Add(AttachmentNumber);
            Children.Add(AttachmentContent);
            Children.Add(EndAttachment);
        }

        internal NewAttachmentMarker NewAttachment;
        internal AttachmentNumberTag AttachmentNumber;
        internal AttachmentContent AttachmentContent;
        internal EndAttachMarker EndAttachment;

        internal static bool IsAttachmentBeginTag(PropertyTag propertyTag)
        {
            return propertyTag.Data == PropertyTag.NewAttach;
        }

        public override void WriteToCompoundFile(CompoundFileBuild build)
        {
            build.StartWriteAttachment();
            base.WriteToCompoundFile(build);
            build.EndWriteAttachment();
        }
    }
}
