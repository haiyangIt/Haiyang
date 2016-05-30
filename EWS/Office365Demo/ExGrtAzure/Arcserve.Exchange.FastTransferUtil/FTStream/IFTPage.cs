using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.FTStream
{
    public interface IFTPage
    {
        int CurrentPageIndex { get; }
        byte[] GetNextPageBuffer();
        bool IsLastPage { get; }
    }
}
