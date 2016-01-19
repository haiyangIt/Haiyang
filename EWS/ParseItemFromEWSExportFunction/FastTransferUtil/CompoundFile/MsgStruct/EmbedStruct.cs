using FastTransferUtil.CompoundFile.MsgStruct.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices.ComTypes;

namespace FastTransferUtil.CompoundFile.MsgStruct
{
    public class EmbedStruct : BaseStruct
    {
        public MessageContentStruct MessageContent;

        protected override string Name
        {
            get
            {
                return "EmbedStruct";
            }
        }

        public EmbedStruct(BaseStruct parentStruct) : base(parentStruct)
        {
        }

        private string GetStorageName()
        {
            return "__substg1.0_3701000D";
        }

        protected override void CreateSelfStorageForBuild()
        {
            if (MessageContent.Properties.Properties.Count > 0 || MessageContent.RecipientProperties.Count > 0 || MessageContent.AttachmentProperties.Count > 0)
                Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(), true, ParentStruct.Storage);
        }

        protected override void BuildEx()
        {
            if (MessageContent.Properties.Properties.Count > 0 || MessageContent.RecipientProperties.Count > 0 || MessageContent.AttachmentProperties.Count > 0)
                MessageContent.BuildInternal();
        }

        public MessageContentStruct CreateMessageContent()
        {
            MessageContent = new MessageContentStruct(this);
            return MessageContent;
        }

        protected override void Release(bool hasError)
        {
            if (Storage != null)
            {
                if (!hasError && !isParser)
                    Storage.Commit(0);
                CompoundFileUtil.Instance.ReleaseComObj(Storage);
            }
        }

        protected override void ParserEx()
        {
            GetStorageForParser();
            MessageContent = new MessageContentStruct(this);
            MessageContent.ParserInternal();
        }

        protected override void GetStorageForParser()
        {
            if (Storage == null)
            {
                Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(), false, ParentStruct.Storage);
            }
        }

        public override bool Compare(object other, StringBuilder result, int indent)
        {
            var desStruct = other as EmbedStruct;
            if (desStruct == null)
            {
                BaseStruct.SetMessage(result, indent, "{0} Type is not EmbedStruct.", Name);
                return false;
            }

            bool returnResult = true;

            var isSame = base.Compare(other, result, indent);
            if (!isSame) returnResult = false;

            if(MessageContent != null)
            {
                if (!MessageContent.Compare(desStruct.MessageContent, result, indent + 2))
                {
                    returnResult = false;
                }
            }
            return returnResult;
        }
    }
}
