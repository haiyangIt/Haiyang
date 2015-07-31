using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public interface IFTTreeLeaf : IFTSerialize
    {
        string GetLeafString();
        int GetLeafByte(IFTStreamWriter writer);
        int BytesCount { get; }
    }
}
