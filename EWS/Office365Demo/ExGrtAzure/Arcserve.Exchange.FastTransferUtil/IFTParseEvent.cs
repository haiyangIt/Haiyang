using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil
{
    public interface IFTParseEvent
    {
        void BeginParse();

        void BeginParseOutput();

        void AfterParse();

        void AfterParseOutput();
        
    }
}
