using FastTransferUtil.CompoundFile.MsgStruct.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        internal override IStorage Storage
        {
            get
            {
                if (base.Storage == null)
                {
                    base.Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(), true, ParentStruct.Storage);
                }
                return base.Storage;
            }

            set
            {
                throw new InvalidProgramException();
            }
        }

        protected override void BuildEx()
        {
            MessageContent.BuildInternal();
        }

        public MessageContentStruct CreateMessageContent()
        {
            MessageContent = new MessageContentStruct(this);
            return MessageContent;
        }

        protected override void Release(bool hasError)
        {
            if (base.Storage != null)
            {
                if (!hasError)
                    base.Storage.Commit(0);
                CompoundFileUtil.Instance.ReleaseComObj(base.Storage);
            }
        }
    }
}
