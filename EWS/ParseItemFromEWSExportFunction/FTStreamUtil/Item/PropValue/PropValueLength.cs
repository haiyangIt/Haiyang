using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public class PropValueLength : FTNodeLeaf<UInt32>
    {
        protected override uint ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadUInt32();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X8");
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
                return FTStreamConst.UInt32Size;
            }
        }
    }
}
