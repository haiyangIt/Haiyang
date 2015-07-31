using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{

    interface IFTStreamRoot : ILog
    {

    }

    class FTStreamRootParse
    {
        public static IFTStreamRoot ParseFTStreamRoot(byte[] buffer)
        {
            int pos = 0;
            return MessageContent.CreateMessageContent(buffer, ref pos);
        }

        public static void Output(IFTStreamRoot streamRoot)
        {
            StringBuilder sb = new StringBuilder();
            FTStreamParseContext.Instance.SetStringBuilder(sb);
            streamRoot.LogInfo(sb);
            
        }

    }
}
