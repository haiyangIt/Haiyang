using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil
{
    public abstract class FTNodeLeaf<T> : FTNodeBase, IFTTreeNode , IFTTreeLeaf, IFTParseEvent
    {
        public T Data { get; private set; }

        protected override void ParseNode(IFTStreamReader reader)
        {
            Data = ReadLeafData(reader);
            CheckData(Data);
        }

        internal void Init(T data)
        {
            Data = data;
            CheckData(Data);
        }

        protected abstract T ReadLeafData(IFTStreamReader reader);

        protected virtual void CheckData(T Data)
        {

        }

        public override IList<IFTTreeNode> Children
        {
            get { return new List<IFTTreeNode>(0); }
        }

        public abstract string GetLeafString();

        public abstract int WriteLeafData(IFTStreamWriter writer);
    }
}
