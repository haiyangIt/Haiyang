using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public class MvPropType : FTNodeLeaf<UInt16>
    {
        protected override ushort ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadUInt16();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X4");
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
