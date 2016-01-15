using FTStreamUtil;
using FTStreamUtil.FTStream;
using FTStreamUtil.Item;
using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastTransferUtil.CompoundFile.MsgStruct.Helper
{

    internal interface ISpecialValue : IPropValue { }

    internal class SpecialFixProperty : ISpecialValue
    {
        public SpecialFixProperty(int tag, byte[] value)
        {
            PropTag = new PropertyTag((uint)tag);
            Tag = (uint)tag;
            Bytes = value;

            PropValue = new SpecialPropertyValue() { Bytes = this.Bytes, BytesCount = this.BytesCount };
        }

        public byte[] Bytes
        {
            get; set;
        }

        public int BytesCount
        {
            get
            {
                return Bytes.Length;
            }
        }

        public IList<IFTTreeNode> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropInfo PropInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropTag PropTag
        {
            get; set;
        }

        public IValue PropValue
        {
            get; set;
        }

        public uint Tag
        {
            get; set;
        }

        public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
        {
            throw new NotImplementedException();
        }

        public int GetByteData(IFTStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Parse(IFTStreamReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteToCompoundFile(CompoundFileBuild build)
        {
            throw new NotImplementedException();
        }

        class SpecialPropertyValue : IValue
        {
            public byte[] Bytes
            {
                get; set;
            }

            public int BytesCount
            {
                get; set;
            }

            public byte[] BytesForMsg
            {
                get
                {
                    return Bytes;
                }
            }

            public int BytesCountForMsg
            {
                get
                {
                    return BytesCount;
                }
            }

            public IList<IFTTreeNode> Children
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
            {
                throw new NotImplementedException();
            }

            public int GetByteData(IFTStreamWriter writer)
            {
                throw new NotImplementedException();
            }

            public void Parse(IFTStreamReader reader)
            {
                throw new NotImplementedException();
            }

            public void WriteToCompoundFile(CompoundFileBuild build)
            {
                throw new NotImplementedException();
            }
        }
    }

    internal class SpecialVarBinaryProperty : SpecialVarBaseProperty
    {
        public SpecialVarBinaryProperty(uint tag, byte[] value) : base(tag, value)
        {

        }
    }

    internal class SpecialVarStringProperty : SpecialVarBaseProperty
    {
        public SpecialVarStringProperty(uint tag, byte[] value) : base(tag, value)
        {

        }
        public override int BytesCount
        {
            get
            {
                return Bytes.Length + 2;
            }
        }
    }

    internal class SpecialVarBaseProperty : ISpecialValue
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value">doesnot contain the terminate character.</param>
        public SpecialVarBaseProperty(uint tag, byte[] value)
        {
            Bytes = value;
            Tag = tag;
            PropTag = new PropertyTag(tag);
            PropValue = new SpecialVarStrValue(Bytes, BytesCount);
        }

        public virtual byte[] Bytes
        {
            get; set;
        }

        public virtual int BytesCount
        {
            get
            {
                return Bytes.Length;
            }
        }

        public IList<IFTTreeNode> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropInfo PropInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropTag PropTag
        {
            get; set;
        }

        public IValue PropValue
        {
            get; set;
        }

        public uint Tag
        {
            get; set;
        }

        public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
        {
            throw new NotImplementedException();
        }

        public int GetByteData(IFTStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Parse(IFTStreamReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteToCompoundFile(CompoundFileBuild build)
        {
            throw new NotImplementedException();
        }


    }

    class SpecialVarStrValue : IValue
    {
        public SpecialVarStrValue(byte[] bytes, int byteCount)
        {
            Bytes = bytes;
            BytesCount = byteCount;
        }
        public byte[] Bytes
        {
            get; set;
        }

        public int BytesCount
        {
            get; set;
        }

        public int BytesCountForMsg
        {
            get
            {
                return BytesCount;
            }
        }

        public byte[] BytesForMsg
        {
            get
            {
                return Bytes;
            }
        }

        public IList<IFTTreeNode> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
        {
            throw new NotImplementedException();
        }

        public int GetByteData(IFTStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Parse(IFTStreamReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteToCompoundFile(CompoundFileBuild build)
        {
            throw new NotImplementedException();
        }
    }
}
