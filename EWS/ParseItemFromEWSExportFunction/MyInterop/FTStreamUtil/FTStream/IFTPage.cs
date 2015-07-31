using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.FTStream
{
    public interface IFTPage
    {
        int CurrentPageIndex { get; }
        byte[] GetNextPageBuffer();
        bool IsLastPage { get; }
    }
}
