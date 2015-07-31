using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class EmbeddedMessage: ILog
    {
        public IMarker StartEmbed;
        public MessageContent MsgContent;
        public IMarker EndEmbed;

        private EmbeddedMessage() { }

        private bool TryParse(byte[] buffer, ref int pos)
        {
            IMarker marker;
            if (Marker.TryCreateMarker(buffer, ref pos, out marker) && Marker.IsSpecificMarker(marker, Marker.StartEmbed))
            {
                StartEmbed = marker;
            }
            else
                return false;

            MsgContent = MessageContent.CreateMessageContent(buffer, ref pos);

            if (Marker.TryCreateMarker(buffer, ref pos, out marker) && Marker.IsSpecificMarker(marker, Marker.EndEmbed))
            {
                EndEmbed = marker;
            }
            else
                throw new ArgumentException("Embeddedmessage end parse error.");

            return true;
        }

        internal static bool TryParse(byte[] buffer, ref int pos, out EmbeddedMessage embedMessage)
        {
            embedMessage = new EmbeddedMessage();
            return embedMessage.TryParse(buffer, ref pos);
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("EmbeddedMessage:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("StartEmbed:");
            StartEmbed.LogInfo(logBuilder);


            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("MsgContent:");
            MsgContent.LogInfo(logBuilder);

            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("EndEmbed:");
            EndEmbed.LogInfo(logBuilder);
            
            FTStreamParseContext.Instance.ResetIndent();
        }
    }
}
