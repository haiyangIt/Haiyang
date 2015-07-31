using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item
{
    public class MessageContent : FTNodeBase
    {
        internal PropList Props;
        internal MessageChildren MsgChildren;
        public MessageContent()
            : base()
        {
            Props = FTFactory.Instance.CreatePropList();
            MsgChildren = FTFactory.Instance.CreateMessageChildren();
            Children.Add(Props);
            Children.Add(MsgChildren);
        }

    }
}
