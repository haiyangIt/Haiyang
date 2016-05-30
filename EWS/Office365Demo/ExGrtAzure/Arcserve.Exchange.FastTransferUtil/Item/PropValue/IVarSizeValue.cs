using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.Item.PropValue
{
    public interface IVarSizeValue : IValue, IFTTreeNode
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

        public virtual byte[] BytesForMsg
        {
            get
            {
                return Bytes;
            }
        }

        public virtual int BytesCountForMsg
        {
            get
            {
                return BytesCount;
            }
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

        protected override string ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadUnicodeStringWithCodePage((int)_length.Data, (int)codepage);
        }

        public override string GetLeafString()
        {
            return Data.Substring(0, Data.Length - 1);
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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

        public override byte[] BytesForMsg
        {
            get
            {
                if (BytesCountForMsg == 0)
                    return new byte[0];
                var result = new byte[BytesCountForMsg - 2];
                Array.Copy(Bytes, result, BytesCountForMsg - 2);
                return result;
            }
        }

        public override int BytesCountForMsg
        {
            get
            {
                return BytesCount;
            }
        }
    }

    public class VarString8Value : VarBaseValue<string>, IVarSizeValue
    {
        public VarString8Value(VarPropType propType, PropValueLength length) : base(propType, length) { }

        protected override string ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadAnsiString((int)_length.Data);
        }

        public override string GetLeafString()
        {
            return Data.Substring(0, Data.Length - 1);
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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

        protected override byte[] ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadBytes((int)_length.Data);
        }

        public override string GetLeafString()
        {
            StringBuilder sb = new StringBuilder(Data.Length + 10);
            sb.Append("Binary:[");
            foreach (byte b in Data)
            {
                sb.Append(b.ToString("X2")).Append(" ");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
