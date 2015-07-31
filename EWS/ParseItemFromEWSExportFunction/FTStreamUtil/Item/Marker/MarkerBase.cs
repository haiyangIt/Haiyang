﻿using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.Marker
{
    public abstract class MarkerBase : FTNodeLeaf<UInt32>, IFTTransferUnit
    {
        protected abstract UInt32 SpecificData { get; }

        protected abstract string Name { get; }

        protected override uint ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadUInt32();
        }

        protected override void CheckData(uint Data)
        {
            if (Data != SpecificData)
                throw new ArgumentException(string.Format("{0} Data is wrong:{0}", Name, Data.ToString("X4")));
        }

        public override string GetLeafString()
        {
            return Data.ToString("X8");
        }

        public override int GetLeafByte(IFTStreamWriter writer)
        {
            int count = writer.Write(Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return FTStreamConst.UInt32Size;
            }
        }

        public IFTTransferUnit GetUnit()
        {
            Init(SpecificData);
            return this;
        }
    }
}
