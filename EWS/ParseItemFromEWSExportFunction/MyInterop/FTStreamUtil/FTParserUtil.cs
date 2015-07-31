using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public class FTParserUtil
    {
        private List<byte[]> _allBytes;
        public FTParserUtil(List<byte[]> allBytes)
        {
            _allBytes = allBytes;
        }

        public FTMessageTreeRoot Parser()
        {
            FTBufferRead reader = new FTBufferRead(_allBytes);
            FTMessageTreeRoot root = new FTMessageTreeRoot();
            using (IFTStreamReader streamReader = new FTStreamReaderForPage(reader))
            {
                root.Parse(streamReader);
            }
            return root;
        }
    }
}
