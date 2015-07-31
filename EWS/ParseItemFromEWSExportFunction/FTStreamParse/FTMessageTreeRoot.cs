using FTStreamUtil.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public class FTMessageTreeRoot : FTNodeBase
    {
        internal MessageContent MessageContent;
        public FTMessageTreeRoot()
            : base()
        {
            MessageContent = FTFactory.Instance.CreateMessageContent();
            Children.Add(MessageContent);
        }
    }
}
