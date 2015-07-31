using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class PTypInteger32 : ValueBase<UInt32>, ILength, IDispId, IFixedSizeValue, IMarker
    {
        private PTypInteger32(UInt32 value)
            : base(value)
        {

        }

        private PTypInteger32() : base() { }

        protected override ParseFixValue<UInt32> ParseFixValueFunc
        {
            get { return SignToUnSign; }
        }

        private UInt32 SignToUnSign(byte[] buffer, int pos)
        {
            return (UInt32)ParseSerialize.ParseInt32(buffer, pos);
        }

        protected override int PosAddNum
        {
            get { return 4; }
        }

        internal static IDispId CreateDispId(byte[] buffer, ref int pos)
        {
            UInt32 value = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
            pos += 4;
            return new PTypInteger32(value);
        }

        internal static IFixedSizeValue CreateFixedPropValue()
        {
            return new PTypInteger32();
        }

        internal static ILength CreateLength(byte[] buffer, ref int pos)
        {
            UInt32 value = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
            pos += 4;
            return new PTypInteger32(value);
        }

        public void ParseDispId(byte[] buffer, ref int pos)
        {
            ParseValue(buffer, ref pos);
        }

        public override string ToString()
        {
            return string.Format("Int32:[{0}]", Value.ToString("X8"));
        }

        public static bool JudgeIsMarker(byte[] buffer, ref int pos, out IMarker marker)
        {
            UInt32 value = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
            if (Marker.JudgeIsMarker(value))
            {
                pos += 4;
                marker = new PTypInteger32(value);
                return true;
            }
            marker = null;
            return false;
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("Marker:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).AppendLine(this.ToString());
            FTStreamParseContext.Instance.ResetIndent();
        }
    }
}
