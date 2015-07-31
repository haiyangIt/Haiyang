using FTStreamUtil.Build;
using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public class MetaPropertyFxDelTag : MetaPropertyBase
    {
        private const UInt32 _fxDelTag = 0x40160003;

        private const UInt32 _subObjectIsRecipient = 0x0E12000D;
        private const UInt32 _subObjectIsAttachment = 0x0E13000D;

        public override bool IsTagRight(PropertyTag propertyTag)
        {
            return propertyTag.Data == _fxDelTag;
        }

        public static IFTTransferUnit GetAttachmentMetaDelTag()
        {
            UInt64 value = _fxDelTag << 32 | _subObjectIsAttachment;
            return new ByteArrayTransferUnit(BitConverter.GetBytes(value));
        }

        public static IFTTransferUnit GetRecipientMetaDelTag()
        {
            UInt64 value = _fxDelTag << 32 | _subObjectIsRecipient;
            return new ByteArrayTransferUnit(BitConverter.GetBytes(value));
        }
    }
}
