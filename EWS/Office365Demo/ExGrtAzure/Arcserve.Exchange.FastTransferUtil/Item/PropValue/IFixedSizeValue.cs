using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.Item.PropValue
{
    public interface IFixedSizeValue : IValue, IFTTreeNode
    {
    }

    public abstract class FixedBaseSizeValue<T> : FTNodeLeaf<T>, IFixedSizeValue
    {
        protected readonly FixedPropType _propType;
        protected FixedBaseSizeValue(FixedPropType propType)
        {
            _propType = propType;
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
    }

    public class FixedInt16Value : FixedBaseSizeValue<UInt16>
    {
        public FixedInt16Value(FixedPropType propType) : base(propType)
        { }

        protected override ushort ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadUInt16();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X8");
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.UInt16Size;
            }
        }
    }

    public class FixedInt32Value : FixedBaseSizeValue<UInt32>
    {
        public FixedInt32Value(FixedPropType propType)
            : base(propType)
        { }

        protected override UInt32 ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadUInt32();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X8");
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
    }

    public class FixedFloatValue : FixedBaseSizeValue<float>
    {
        public FixedFloatValue(FixedPropType propType)
            : base(propType)
        { }

        protected override float ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadSingle();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.FloatSize;
            }
        }
    }

    public class FixedDoubleValue : FixedBaseSizeValue<double>
    {
        public FixedDoubleValue(FixedPropType propType)
            : base(propType)
        { }

        protected override double ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadDouble();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.DoubleSize;
            }
        }
    }

    public class FixedCurrencyValue : FixedBaseSizeValue<double>
    {
        public FixedCurrencyValue(FixedPropType propType)
            : base(propType)
        { }

        protected override double ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadCurrency();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.CurrencySize;
            }
        }
    }

    public class FixedFloatTimeValue : FixedBaseSizeValue<DateTime>
    {
        public FixedFloatTimeValue(FixedPropType propType)
            : base(propType)
        { }

        protected override DateTime ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadDateTime();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.DateTimeSize;
            }
        }
    }

    public class FixedBooleanValue : FixedBaseSizeValue<bool>
    {
        public FixedBooleanValue(FixedPropType propType)
            : base(propType)
        { }

        protected override bool ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadBoolean();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.BooleanSize;
            }
        }
    }

    public class FixedInt64Value : FixedBaseSizeValue<UInt64>
    {
        public FixedInt64Value(FixedPropType propType)
            : base(propType)
        { }

        protected override UInt64 ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadUInt64();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X8");
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.UInt64Size;
            }
        }
    }

    public class FixedSysTimeValue : FixedBaseSizeValue<DateTime>
    {
        public FixedSysTimeValue(FixedPropType propType)
            : base(propType)
        { }

        protected override DateTime ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadDateTime();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.DateTimeSize;
            }
        }
    }

    public class FixedGuidValue : FixedBaseSizeValue<Guid>
    {
        public FixedGuidValue(FixedPropType propType)
            : base(propType)
        { }

        protected override Guid ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadGuid();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
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
                return FTStreamConst.GuidSize;
            }
        }
    }
}
