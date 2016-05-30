using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.Item
{
    public class RecipientCollection : FTNodeCollection<Recipient>
    {
        public override bool IsTagRight(PropertyTag propertyTag)
        {
            return Recipient.IsRecipientBegin(propertyTag);
        }

        protected override Recipient CreateItem(PropertyTag propertyTag)
        {
            return FTFactory.Instance.CreateRecipient(propertyTag);
        }
    }
}
