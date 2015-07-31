using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public interface IFTParseEvent
    {
        void BeginParse();

        void BeginParseOutput();

        void AfterParse();

        void AfterParseOutput();
        
    }
}
