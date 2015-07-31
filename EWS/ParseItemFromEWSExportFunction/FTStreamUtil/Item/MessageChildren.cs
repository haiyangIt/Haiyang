using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

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
    }
}
