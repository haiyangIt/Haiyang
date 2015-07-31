using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class MessageChildren : ILog
    {
        public MetaProperty FxDelPropWithRecv; // optional
        public List<Recipient> Recipients;// optional
        public MetaProperty FxDelPropWithAttach;// optional
        public List<Attachment> Attachments;// optional
        public bool IsEnd = false;

        private MessageChildren()
        {

        }

        private void ProcessNextElement(byte[] buffer,ref int pos)
        {
            while (pos < buffer.Length)
            {
                UInt32 propertyIdWithType = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                // get fxDelProp
                if (propertyIdWithType == MetaProperty.MetaTagFXDelProp && FxDelPropWithRecv == null)
                {
                    IPropValue metaValue = StreamUtil.ParsePropValue(buffer, ref pos);
                    MetaProperty fxDelPropWithRecv = null;
                    MetaProperty.JudgePropIsMetaProp(metaValue, out fxDelPropWithRecv);
                    FxDelPropWithRecv = fxDelPropWithRecv;
                }

                // get recipients
                else if (propertyIdWithType == Marker.StartRecip)
                {
                    if (Recipients == null)
                        Recipients = new List<Recipient>();
                    Recipient recv = null;
                    if (Recipient.TryCreateRecipient(buffer, ref pos, out recv))
                    {
                        Recipients.Add(recv);
                    }
                }
                // get fxDelProp
                else if (propertyIdWithType == MetaProperty.MetaTagFXDelProp && FxDelPropWithAttach == null)
                {
                    IPropValue metaValue = StreamUtil.ParsePropValue(buffer, ref pos);
                    MetaProperty fxDelPropWithRecv = null;
                    MetaProperty.JudgePropIsMetaProp(metaValue, out fxDelPropWithRecv);
                    FxDelPropWithAttach = fxDelPropWithRecv;
                }
                // get Attachment
                else if (propertyIdWithType == Marker.NewAttach)
                {
                    if (Attachments == null)
                    {
                        Attachments = new List<Attachment>();
                    }

                    Attachment att = null;
                    if (Attachment.TryCreateAttachment(buffer, ref pos, out att))
                    {
                        Attachments.Add(att);
                    }
                }
                else
                {
                    IsEnd = true;
                    break;
                }
            }
        }

        internal static MessageChildren CreateMsgChildren(byte[] buffer, ref int pos)
        {
            MessageChildren result = new MessageChildren();
            result.ProcessNextElement(buffer, ref pos);
            return result;
        }

        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("MessageChildren:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();
            if (FxDelPropWithRecv != null)
                FxDelPropWithRecv.LogInfo(logBuilder);
            if(Recipients != null)
            {
                logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("Recipient:").AppendLine();
                FTStreamParseContext.Instance.IncrementIndent();
                foreach(Recipient re in Recipients)
                {
                    re.LogInfo(logBuilder);
                }
                FTStreamParseContext.Instance.ResetIndent();
            }

            if (FxDelPropWithAttach != null)
            {
                FxDelPropWithAttach.LogInfo(logBuilder);
            }

            if (Attachments != null)
            {
                logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("Attachments:").AppendLine();
                FTStreamParseContext.Instance.IncrementIndent();
                foreach (Attachment re in Attachments)
                {
                    re.LogInfo(logBuilder);
                }
                FTStreamParseContext.Instance.ResetIndent();
            }
            FTStreamParseContext.Instance.ResetIndent();
        }
    }
}
