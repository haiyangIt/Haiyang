using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.CompoundFile;

namespace Arcserve.Exchange.FastTransferUtil
{
    public interface IFTTreeNode : IFTSerialize
    {
        void Parse(IFTStreamReader reader);
        IList<IFTTreeNode> Children { get; }
        int BytesCount { get; }

        byte[] Bytes { get; }

        void WriteToCompoundFile(CompoundFileBuild build);
    }
}
