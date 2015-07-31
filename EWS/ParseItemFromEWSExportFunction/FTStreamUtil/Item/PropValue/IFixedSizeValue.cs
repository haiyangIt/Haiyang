using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public interface IFixedSizeValue : IFTTreeNode
    {
    }

    public abstract class FixedBaseSizeValue<T> : FTNodeLeaf<T>, IFixedSizeValue
    {
        protected readonly FixedPropType _propType;
        protected FixedBaseSizeValue(FixedPropType propType)
        {
            _propType = propType;
        }

    }

    public class FixedInt16Value : FixedBaseSizeValue<UInt16>
    {
        public FixedInt16Value(FixedPropType propType) : base(propType)
        { }

        protected override ushort ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadUInt16();
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
                return FTStreamConst.UInt16Size;
            }
        }
    }

    public class FixedInt32Value : FixedBaseSizeValue<UInt32>
    {
        public FixedInt32Value(FixedPropType propType)
            : base(propType)
        { }

        protected override UInt32 ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadUInt32();
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
    }

    public class FixedFloatValue : FixedBaseSizeValue<float>
    {
        public FixedFloatValue(FixedPropType propType)
            : base(propType)
        { }

        protected override float ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadSingle();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
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
                return FTStreamConst.FloatSize;
            }
        }
    }

    public class FixedDoubleValue : FixedBaseSizeValue<double>
    {
        public FixedDoubleValue(FixedPropType propType)
            : base(propType)
        { }

        protected override double ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadDouble();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
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
                return FTStreamConst.DoubleSize;
            }
        }
    }

    public class FixedCurrencyValue : FixedBaseSizeValue<double>
    {
        public FixedCurrencyValue(FixedPropType propType)
            : base(propType)
        { }

        protected override double ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadCurrency();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
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
                return FTStreamConst.CurrencySize;
            }
        }
    }

    public class FixedFloatTimeValue : FixedBaseSizeValue<DateTime>
    {
        public FixedFloatTimeValue(FixedPropType propType)
            : base(propType)
        { }

        protected override DateTime ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadDateTime();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
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
                return FTStreamConst.DateTimeSize;
            }
        }
    }

    public class FixedBooleanValue : FixedBaseSizeValue<bool>
    {
        public FixedBooleanValue(FixedPropType propType)
            : base(propType)
        { }

        protected override bool ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadBoolean();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
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
                return FTStreamConst.BooleanSize;
            }
        }
    }

    public class FixedInt64Value : FixedBaseSizeValue<UInt64>
    {
        public FixedInt64Value(FixedPropType propType)
            : base(propType)
        { }

        protected override UInt64 ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadUInt64();
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
                return FTStreamConst.UInt64Size;
            }
        }
    }

    public class FixedSysTimeValue : FixedBaseSizeValue<DateTime>
    {
        public FixedSysTimeValue(FixedPropType propType)
            : base(propType)
        { }

        protected override DateTime ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadDateTime();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
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
                return FTStreamConst.DateTimeSize;
            }
        }
    }

    public class FixedGuidValue : FixedBaseSizeValue<Guid>
    {
        public FixedGuidValue(FixedPropType propType)
            : base(propType)
        { }

        protected override Guid ParseLeafData()
        {
            return FTStreamParseContext.Instance.Parser.ReadGuid();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
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
                return FTStreamConst.GuidSize;
            }
        }
    }
}
