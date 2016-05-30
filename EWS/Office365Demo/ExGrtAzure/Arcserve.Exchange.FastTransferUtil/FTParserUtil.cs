using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil
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

        public static int FindChildCount<T>(IFTTreeNode root)
        {
            Queue<IFTTreeNode> queues = new Queue<IFTTreeNode>();
            queues.Enqueue(root);

            while (queues.Count > 0)
            {
                var item = queues.Dequeue();

                var children = item.Children;
                if (children.Count > 0)
                {
                    foreach (var child in children)
                    {
                        if (child is T)
                        {
                            return child.Children.Count;
                        }
                        queues.Enqueue(child);
                    }
                }
            }

            throw new ArgumentException("bin parse wrong. please check code.");
        }
    }
}
