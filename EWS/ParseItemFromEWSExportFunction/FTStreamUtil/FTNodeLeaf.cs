using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public abstract class FTNodeLeaf<T> : FTNodeBase, IFTTreeNode , IFTTreeLeaf, IFTParseEvent
    {
        public T Data { get; private set; }

        protected override void ParseNode()
        {
            Data = ParseLeafData();
            CheckData(Data);
        }

        internal void Init(T data)
        {
            Data = data;
            CheckData(Data);
        }

        protected abstract T ParseLeafData();

        protected virtual void CheckData(T Data)
        {

        }

        public override IList<IFTTreeNode> Children
        {
            get { return new List<IFTTreeNode>(0); }
        }

        public abstract string GetLeafString();

        public abstract int GetLeafByte(IFTStreamWriter writer);
    }
}
