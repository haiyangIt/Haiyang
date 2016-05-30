using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil
{
    public interface IFTTreeLeaf : IFTSerialize
    {
        string GetLeafString();
        int WriteLeafData(IFTStreamWriter writer);
        int BytesCount { get; }
    }
}
