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
        int WriteLeafData(IFTStreamWriter writer);
        int BytesCount { get; }
    }
}
