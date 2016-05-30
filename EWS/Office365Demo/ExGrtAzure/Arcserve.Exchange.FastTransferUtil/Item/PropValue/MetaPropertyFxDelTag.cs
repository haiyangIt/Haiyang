using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.Item.PropValue
{
    public class MetaPropertyFxDelTag : MetaPropertyBase
    {
        private const UInt32 _fxDelTag = 0x40160003;

        internal const UInt32 SubObjectIsRecipient = 0x0E12000D;
        internal const UInt32 SubObjectIsAttachment = 0x0E13000D;

        internal bool HasData { get; private set; }

        public override bool IsTagRight(PropertyTag propertyTag)
        {
            if (propertyTag.Data == _fxDelTag)
            {
                HasData = true;
                return true;
            }
            else
            {
                HasData = false;
                return false;
            }
        }

        internal bool IsRecvTag()
        {
            return ((FTNodeLeaf<UInt32>)(Children[0].Children[2])).Data == SubObjectIsRecipient;
        }

        internal bool IsAttachTag()
        {
            return ((FTNodeLeaf<UInt32>)(Children[0].Children[2])).Data == SubObjectIsAttachment;
        }

        public static IFTTransferUnit GetAttachmentMetaDelTag()
        {
            UInt64 value = ((UInt64)SubObjectIsAttachment) << 32 | ((UInt64)_fxDelTag);
            return new ByteArrayTransferUnit(BitConverter.GetBytes(value));
        }

        public static IFTTransferUnit GetRecipientMetaDelTag()
        {
            UInt64 value = ((UInt64)SubObjectIsRecipient) << 32 | ((UInt64)_fxDelTag);
            return new ByteArrayTransferUnit(BitConverter.GetBytes(value));
        }
    }
}
