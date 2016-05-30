using Arcserve.Exchange.FastTransferUtil.CompoundFile;
using Arcserve.Exchange.FastTransferUtil.Item.Marker;
using Arcserve.Exchange.FastTransferUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.Item
{
    public class Recipient : FTNodeBase
    {
        internal StartRecipientMarker StartRecipientMarker;
        internal PropList RecvPropList;
        internal EndRecipientMarker EndRecipientMarker;

        public Recipient()
            : base()
        {
            StartRecipientMarker = FTFactory.Instance.CreateStartRecipientMarker();
            RecvPropList = FTFactory.Instance.CreatePropList();
            EndRecipientMarker = FTFactory.Instance.CreateEndRecipientMarker();

            Children.Add(StartRecipientMarker);
            Children.Add(RecvPropList);
            Children.Add(EndRecipientMarker);
        }

        internal static bool IsRecipientBegin(PropertyTag propertyTag)
        {
            return propertyTag.Data == PropertyTag.StartRecip;
        }

        public override void WriteToCompoundFile(CompoundFileBuild build)
        {
            build.StartWriteRecipient();
            base.WriteToCompoundFile(build);
            build.EndWriteRecipient();
        }
    }
}
