using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public class FixedPropType : FTNodeLeaf<UInt16>
    {
        protected override ushort ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadUInt16();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X4");
        }

        public override int GetLeafByte(IFTStreamWriter writer)
        {
            int count = writer.Write(Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return FTStreamConst.UInt16Size;
            }
        }
    }
}
