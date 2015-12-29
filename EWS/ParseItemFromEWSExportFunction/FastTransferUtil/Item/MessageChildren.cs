using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using FastTransferUtil.CompoundFile;

namespace FTStreamUtil.Item
{
    public class MessageChildren : FTNodeBase
    {
        public MetaPropertyFxDelTag MetaTagFXDelPropBeforeRecv;
        public RecipientCollection Recipients;
        public MetaPropertyFxDelTag MetaTagFXDelPropBeforeAttach;
        public AttachmentCollection Attachments;
        public MessageChildren()
            : base()
        {
            MetaTagFXDelPropBeforeRecv = FTFactory.Instance.CreateFxDelMetaProperty();
            Recipients = FTFactory.Instance.CreateRecipientCollection();
            MetaTagFXDelPropBeforeAttach = FTFactory.Instance.CreateFxDelMetaProperty();
            Attachments = FTFactory.Instance.CreateAttachmentCollection();
            Children.Add(MetaTagFXDelPropBeforeRecv);
            Children.Add(Recipients);
            Children.Add(MetaTagFXDelPropBeforeAttach);
            Children.Add(Attachments);
        }

        public override void AfterParse()
        {
            if (MetaTagFXDelPropBeforeRecv.HasData)
            {
                if (!MetaTagFXDelPropBeforeRecv.IsRecvTag())
                {
                    throw new ArgumentException(string.Format("MetaTagFxDelProperty is not recipient:[{0}].", ((FTNodeLeaf<UInt32>)(MetaTagFXDelPropBeforeRecv.Children[3])).Data));
                }
            }

            if(MetaTagFXDelPropBeforeAttach.HasData)
            {
                if (!MetaTagFXDelPropBeforeAttach.IsAttachTag())
                {
                    throw new ArgumentException(string.Format("MetaTagFxDelProperty is not attachment:[{0}].", ((FTNodeLeaf<UInt32>)(MetaTagFXDelPropBeforeRecv.Children[3])).Data));
                }
            }

            base.AfterParse();
        }
    }
}
