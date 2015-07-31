using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item
{
    public class AttachmentContent : FTNodeBase
    {
        internal PropList AttachmentPropList;
        internal EmbeddedMessage EmbeddedMessage;

        public AttachmentContent()
            : base()
        {
            AttachmentPropList = FTFactory.Instance.CreatePropList();
            EmbeddedMessage = FTFactory.Instance.CreateEmbeddedMessage();

            Children.Add(AttachmentPropList);
            Children.Add(EmbeddedMessage);
        }
    }
}
