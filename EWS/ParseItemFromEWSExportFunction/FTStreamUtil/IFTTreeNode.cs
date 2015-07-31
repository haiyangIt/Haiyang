using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public interface IFTTreeNode : IFTSerialize
    {
        void Parse();
        IList<IFTTreeNode> Children { get; }
        int BytesCount { get; }

    }
}
