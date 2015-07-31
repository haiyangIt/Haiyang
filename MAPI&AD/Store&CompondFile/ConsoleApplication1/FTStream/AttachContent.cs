using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class AttachmentContent : IPropertyProcess,ILog
    {
        private AttachmentContent() { }
        public PropList AttachPropList;
        public EmbeddedMessage EmbedMessage; // optional

        private bool _isEnd;

        private bool TryParse(byte[] buffer, ref int pos)
        {
            while (pos < buffer.Length)
            {
                ElementTypeProcessBase.ParseByElementType(buffer, ref pos, this);
                if (_isEnd)
                    break;
            }
            return true;
        }

        internal static bool TryParse(byte[] buffer, ref int pos, out AttachmentContent AttachContent)
        {
            AttachContent = new AttachmentContent();
            return AttachContent.TryParse(buffer, ref pos);
        }

        public void ParseMarker(byte[] buffer, ref int pos)
        {
            IMarker marker;
            if (Marker.TryCreateMarker(buffer, ref pos, out marker) )
            {
                pos -= 4;
                if (Marker.IsSpecificMarker(marker, Marker.EndAttach))
                    _isEnd = true;
                else if(Marker.IsSpecificMarker(marker,Marker.StartEmbed))
                {
                    if (!EmbeddedMessage.TryParse(buffer, ref pos, out EmbedMessage))
                    {
                        throw new ArgumentException("Parse embeddedMessage error.");
                    }
                }
                else
                    throw new ArgumentException("Parse AttachmentContent error.");
            }
            else
                throw new ArgumentException("Parse AttachmentContent error.");
        }

        public void ParseMetaProperty(byte[] buffer, ref int pos)
        {
            throw new ArgumentException("Parse attachment content error.");
        }

        public void ParsePropValue(byte[] buffer, ref int pos)
        {
            if (AttachPropList == null)
                AttachPropList = PropList.CreatePropertyList();
            PropList.AddProperty(buffer, ref pos, ref AttachPropList);
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("AttachContent:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            if(AttachPropList != null)
            {
                logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("AttachPropList:");
                AttachPropList.LogInfo(logBuilder);
            }
            if (EmbedMessage != null)
            {
                logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("EmbedMessageInAttachment:");
                EmbedMessage.LogInfo(logBuilder);
            }
            FTStreamParseContext.Instance.ResetIndent();
        }
    }
}
