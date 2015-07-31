using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public interface IVarSizeValue : IFTTreeNode
    {
    }

    public abstract class VarBaseValue<T> : FTNodeLeaf<T>, IVarSizeValue
    {
        protected readonly PropValueLength _length;
        protected readonly VarPropType _propType;
        protected VarBaseValue(VarPropType propType, PropValueLength length)
        {
            _length = length;
            _propType = propType;
        }

    }

    public class VarStringValue : VarBaseValue<string>, IVarSizeValue
    {
        private uint codepage
        {
            get
            {
                return (UInt32)(_propType.Data & 0x0FFF);
            }
        }

        public VarStringValue(VarPropType propType, PropValueLength length) : base(propType, length) {
            
        }

        protected override string ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadUnicodeStringWithCodePage((int)_length.Data, (int)codepage);
        }

        public override string GetLeafString()
        {
            return Data.Substring(0, Data.Length - 1);
        }

        public override int GetLeafByte(IFTStreamWriter writer)
        {
            int count = writer.WriteUnicodeStringWithCodePage(Data, _length.Data, codepage);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return (int)_length.Data;
            }
        }
    }

    public class VarString8Value : VarBaseValue<string>, IVarSizeValue
    {
        public VarString8Value(VarPropType propType, PropValueLength length) : base(propType, length) { }

        protected override string ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadAnsiString((int)_length.Data);
        }

        public override string GetLeafString()
        {
            return Data.Substring(0, Data.Length - 1);
        }

        public override int GetLeafByte(IFTStreamWriter writer)
        {
            int count = writer.WriteAnsiString(Data, _length.Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return (int)_length.Data;
            }
        }
    }

    public class VarServerIdValue : VarBinaryValue
    {
        public VarServerIdValue(VarPropType propType, PropValueLength length) : base(propType, length) { }
    }

    public class VarBinaryValue : VarBaseValue<byte[]>, IVarSizeValue
    {
        public VarBinaryValue(VarPropType propType, PropValueLength length) : base(propType, length) { }

        protected override byte[] ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadBytes((int)_length.Data);
        }

        public override string GetLeafString()
        {
            StringBuilder sb = new StringBuilder(Data.Length + 10);
            sb.Append("Binary:[");
            foreach (byte b in Data)
            {
                sb.Append(b);
            }
            sb.Append("]");
            return sb.ToString();
        }

        public override int GetLeafByte(IFTStreamWriter writer)
        {
            int count = writer.WriteBytes(Data, _length.Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return (int)_length.Data;
            }
        }
    }

    public class VarObjectValue : VarBinaryValue
    {
        public VarObjectValue(VarPropType propType, PropValueLength length) : base(propType, length) { }
    }
}
