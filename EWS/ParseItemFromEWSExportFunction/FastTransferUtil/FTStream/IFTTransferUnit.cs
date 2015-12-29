using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.FTStream
{
    public interface IFTTransferUnit
    {
        int BytesCount { get; }
        byte[] Bytes { get; }
        UInt32 Tag { get; }
    }
}
