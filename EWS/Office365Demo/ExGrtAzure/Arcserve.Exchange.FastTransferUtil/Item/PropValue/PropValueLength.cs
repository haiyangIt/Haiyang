using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.Item.PropValue
{
    public class PropValueLength : FTNodeLeaf<UInt32>
    {
        protected override uint ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadUInt32();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X8");
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
                return FTStreamConst.UInt32Size;
            }
        }
    }
}
