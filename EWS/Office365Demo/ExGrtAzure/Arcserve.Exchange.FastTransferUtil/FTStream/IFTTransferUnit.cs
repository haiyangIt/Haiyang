using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.FTStream
{
    public interface IFTTransferUnit
    {
        int BytesCount { get; }
        byte[] Bytes { get; }
        UInt32 Tag { get; }
    }
}
