using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class Recipient : IPropertyProcess,ILog
    {
        public IMarker StartRecip;
        public PropList RecipPropList;
        public IMarker EndRecip;

        private bool _isEnd = false;
        private Recipient()
        {

        }

        private bool TryParse(byte[] buffer,ref int pos)
        {
            IMarker marker;
            if (Marker.TryCreateMarker(buffer, ref pos, out marker) && Marker.IsSpecificMarker(marker, Marker.StartRecip))
            {
                StartRecip = marker;
            }
            else
                return false;

            while (pos < buffer.Length)
            {
                ElementTypeProcessBase.ParseByElementType(buffer, ref pos, this);
                if (_isEnd)
                    break;
            }

            return true;
        }

        internal static bool TryCreateRecipient(byte[] buffer, ref int pos, out Recipient recv)
        {
            recv = new Recipient();
            if (recv.TryParse(buffer, ref pos))
                return true;
            recv = null;
            return false;
        }

        public void ParseMarker(byte[] buffer, ref int pos)
        {
            IMarker marker;
            if (Marker.TryCreateMarker(buffer, ref pos, out marker) && Marker.IsSpecificMarker(marker, Marker.EndToRecip))
            {
                EndRecip = marker;
                _isEnd = true;
            }
            else
                throw new ArgumentException("Parse recipient error.");
        }

        public void ParseMetaProperty(byte[] buffer, ref int pos)
        {
            throw new ArgumentException("Recipient parse error.");
        }

        public void ParsePropValue(byte[] buffer, ref int pos)
        {
            if (RecipPropList == null)
                RecipPropList = PropList.CreatePropertyList();
            PropList.AddProperty(buffer, ref pos, ref RecipPropList);
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("Recipient:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            StartRecip.LogInfo(logBuilder);
            RecipPropList.LogInfo(logBuilder);
            EndRecip.LogInfo(logBuilder);
            FTStreamParseContext.Instance.ResetIndent();
        }

    }
}
