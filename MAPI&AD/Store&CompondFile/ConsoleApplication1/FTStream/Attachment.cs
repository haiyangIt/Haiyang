using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class Attachment:ILog
    {
        public IMarker NewAttachMarker;
        public FixPropType_PropInfo_FixedSizeValue AttachNumber;
        public AttachmentContent AttachContent;
        public IMarker EndAttachMarker;

        internal static bool TryCreateAttachment(byte[] buffer, ref int pos, out Attachment att)
        {
            att = new Attachment();
            return att.TryParse(buffer, ref pos);
            
        }

        private bool TryParse(byte[] buffer, ref int pos)
        {
            IMarker marker;
            if (Marker.TryCreateMarker(buffer, ref pos, out marker) && Marker.IsSpecificMarker(marker, Marker.NewAttach))
            {
                NewAttachMarker = marker;
            }
            else
                return false;

            IPropValue attachCount = StreamUtil.ParsePropValue(buffer, ref pos);
            AttachNumber = attachCount as FixPropType_PropInfo_FixedSizeValue;
            if (AttachNumber == null)
                throw new ArgumentException("Attach number parse error.");

            if (AttachNumber.PropIdWithType != 0x0E210003)
                throw new ArgumentException("Attach number parse error.");

            AttachmentContent.TryParse(buffer, ref pos, out AttachContent);

            if (Marker.TryCreateMarker(buffer, ref pos, out marker) && Marker.IsSpecificMarker(marker, Marker.EndAttach))
            {
                EndAttachMarker = marker;
            }
            else
                throw new ArgumentException("Attach end parse error.");

            return true;
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("Attachment:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("NewAttachMarker:");
            NewAttachMarker.LogInfo(logBuilder);

            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("AttachNumber:");
            AttachNumber.LogInfo(logBuilder);

            AttachContent.LogInfo(logBuilder);

            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("EndAttachMarker:");
            EndAttachMarker.LogInfo(logBuilder);
            FTStreamParseContext.Instance.ResetIndent();
        }
    }

}
