using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class MessageContent : IPropertyProcess, IFTStreamRoot
    {
        private MessageContent() { }

        public PropList PropertyList;
        public MessageChildren MsgChildren;
        public bool IsEnd;

        public void ParseMessageContent(byte[] buffer, ref int pos)
        {
            while (pos < buffer.Length)
            {
                ElementTypeProcessBase.ParseByElementType(buffer, ref pos, this);
                if (IsEnd)
                    break;
            }
        }


        public void ParseMarker(byte[] buffer, ref int pos)
        {
            MsgChildren = MessageChildren.CreateMsgChildren(buffer, ref pos);
            IsEnd = MsgChildren.IsEnd;
        }

        public void ParseMetaProperty(byte[] buffer, ref int pos)
        {
            MsgChildren = MessageChildren.CreateMsgChildren(buffer, ref pos);
        }

        public void ParsePropValue(byte[] buffer, ref int pos)
        {
            if (PropertyList == null)
                PropertyList = PropList.CreatePropertyList();
            PropList.AddProperty(buffer, ref pos, ref PropertyList);
        }

        internal static MessageContent CreateMessageContent(byte[] buffer, ref int pos)
        {
            MessageContent msgContent = new MessageContent();
            msgContent.ParseMessageContent(buffer, ref pos);
            return msgContent;
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("MessageContent:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            PropertyList.LogInfo(logBuilder);
            MsgChildren.LogInfo(logBuilder);
            FTStreamParseContext.Instance.ResetIndent();
        }
    }
}
