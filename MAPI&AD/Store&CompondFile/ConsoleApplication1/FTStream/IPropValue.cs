using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public interface IPropValue : IElement, ILog
    {
        void ParsePropValue(byte[] buffer, ref int pos);

        IPropInfo PropInfoInPropValue { get; }
        UInt32 PropIdWithType { get; }
    }

    public class FixPropType_PropInfo_FixedSizeValue : IPropValue
    {
        internal readonly IFixedPropType FixedPropType;
        internal IPropInfo PropInfo;
        internal IFixedSizeValue FixedSizeValue;

        private FixPropType_PropInfo_FixedSizeValue(IFixedPropType fixPropType)
        {
            FixedPropType = fixPropType;
        }

        internal static IPropValue ParseFixPropType_PropInfo_FixedSizeValue(IPropType fixPropType, byte[] buffer, ref int pos)
        {
            FixPropType_PropInfo_FixedSizeValue fixedPropValue = new FixPropType_PropInfo_FixedSizeValue(fixPropType as IFixedPropType);
            fixedPropValue.ParsePropValue(buffer, ref pos);
            return fixedPropValue;
        }

        public void ParsePropValue(byte[] buffer, ref int pos)
        {
            PropInfo = StreamUtil.CreatePropInfo(buffer, ref pos);
            FixedSizeValue = StreamUtil.CreateFixedSizeValue(FixedPropType, buffer, ref pos);
        }

        private string _toString;
        public override string ToString()
        {
            if (String.IsNullOrEmpty(_toString))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("FixedPropType:[").Append(FixedPropType).Append("] PropInfo:[").Append(PropInfo).Append("] FixedSizeValue:[").Append(FixedSizeValue).Append("]");
                _toString = sb.ToString();
            }
            return _toString;
        }


        public IPropInfo PropInfoInPropValue
        {
            get { return PropInfo; }
        }


        public UInt32 PropIdWithType
        {
            get { return ((UInt32)PropInfo.PropId << 16) | (((PtypInteger16)FixedPropType).Value); }
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("FixPropValue:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).AppendLine(this.ToString());
            FTStreamParseContext.Instance.ResetIndent();
        }
    }

    public class VarPropType_PropInfo_Length_VarSizeValue : IPropValue
    {
        internal IVarPropType VarPropType;
        internal IPropInfo PropInfo;
        internal ILength Length;
        internal IVarSizeValue VarSizeValue;

        private VarPropType_PropInfo_Length_VarSizeValue(IVarPropType varPropType)
        {
            VarPropType = varPropType;
        }

        internal static IPropValue ParseVarPropType_PropInfo_Length_VarSizeValue(IPropType varPropType, byte[] buffer, ref int pos)
        {
            VarPropType_PropInfo_Length_VarSizeValue varPropValue = new VarPropType_PropInfo_Length_VarSizeValue(varPropType as IVarPropType);
            varPropValue.ParsePropValue(buffer, ref pos);
            return varPropValue;
        }

        public void ParsePropValue(byte[] buffer, ref int pos)
        {
            PropInfo = StreamUtil.CreatePropInfo(buffer, ref pos);
            Length = PTypInteger32.CreateLength(buffer, ref pos);
            VarSizeValue = StreamUtil.CreateVarSizeValue(VarPropType, Length, buffer, ref pos);
        }

        private string _toString;
        public override string ToString()
        {
            if (String.IsNullOrEmpty(_toString))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("VarPropType:[").Append(VarPropType).Append("] PropInfo:[").Append(PropInfo).Append("] Length:[").Append(Length).Append("] VarSizeValue:[").Append(VarSizeValue).Append("]");
                _toString = sb.ToString();
            }
            return _toString;
        }

        public IPropInfo PropInfoInPropValue
        {
            get { return PropInfo; }
        }


        public uint PropIdWithType
        {
            get { return ((UInt32)PropInfo.PropId << 16) | (((PtypInteger16)VarPropType).Value); }
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("VarPropValue:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).AppendLine(this.ToString());
            FTStreamParseContext.Instance.ResetIndent();
        }
    }

    public class MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue : IPropValue
    {
        internal IMvPropType MvPropType;
        public IPropInfo PropInfo;
        public List<IFixSizeValue_Length_VarSizeValue> Length_IFixSizeValue_Length_VarSizeValue; // count is Length;

        private MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue(IMvPropType mvPropType)
        {

        }

        internal static IPropValue ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue(IPropType mvPropType, byte[] buffer, ref int pos)
        {
            throw new NotImplementedException();
        }

        public void ParsePropValue(byte[] buffer, ref int pos)
        {
            throw new NotSupportedException();
            PropInfo = StreamUtil.CreatePropInfo(buffer, ref pos);
            while (true)
            {
                // todo
            }
        }

        public IPropInfo PropInfoInPropValue
        {
            get { return PropInfo; }
        }


        public uint PropIdWithType
        {
            get { throw new NotImplementedException(); }
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("MvPropValue:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).AppendLine(this.ToString());
            FTStreamParseContext.Instance.ResetIndent();
        }
    }

    public class MetaProperty : IPropValue,ILog
    {
        public readonly IPropValue SimplePropValue;

        private MetaProperty(IPropValue simplePropValue)
        {
            SimplePropValue = simplePropValue;
        }

        private static HashSet<UInt32> _metaPropIdHash;
        public static readonly UInt32 MetaTagFXDelProp = 0x40160003;
        public static readonly UInt32 MetaTagEcWarning = 0x400F0003;
        public static readonly UInt32 MetaTagNewFXFolder = 0x40110102;
        public static readonly UInt32 MetaTagIncrSyncGroupId = 0x407C0003;
        public static readonly UInt32 MetaTagIncrementalSyncMessagePartial = 0x407A0003;
        public static readonly UInt32 MetaTagDnPrefix = 0x4008001E;

        private static void InitMetaPropIdHash()
        {
            _metaPropIdHash = new HashSet<UInt32>();
            _metaPropIdHash.Add(MetaTagFXDelProp);
            _metaPropIdHash.Add(MetaTagEcWarning);
            _metaPropIdHash.Add(MetaTagNewFXFolder);
            _metaPropIdHash.Add(MetaTagIncrSyncGroupId);
            _metaPropIdHash.Add(MetaTagIncrementalSyncMessagePartial);
            _metaPropIdHash.Add(MetaTagDnPrefix);
        }

        public static void JudgePropIsMetaProp(IPropValue propValue, out MetaProperty metaProperty)
        {
            if (JudgePropIsMetaProp(propValue.PropIdWithType))
            {
                metaProperty = new MetaProperty(propValue);
                return;
            }
            throw new ArgumentException("Property is not Metaproperty.");
        }

        public static bool JudgePropIsMetaProp(UInt32 propIdWithType)
        {
            if (_metaPropIdHash == null)
                InitMetaPropIdHash();
            return _metaPropIdHash.Contains(propIdWithType);
        }

        public void ParsePropValue(byte[] buffer, ref int pos)
        {
            throw new InvalidOperationException("this function can't be called.");
        }

        public IPropInfo PropInfoInPropValue
        {
            get { return SimplePropValue.PropInfoInPropValue; }
        }


        public uint PropIdWithType
        {
            get { return SimplePropValue.PropIdWithType; }
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("MetaPropValue:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).AppendLine(this.SimplePropValue.ToString());
            FTStreamParseContext.Instance.ResetIndent();
        }
    }
}
