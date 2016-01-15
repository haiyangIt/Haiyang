using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using FastTransferUtil.CompoundFile.MsgStruct.Helper;
using System.Text;
using FTStreamUtil.Item.PropValue;

namespace FastTransferUtil.CompoundFile.MsgStruct
{
    public class MessageContentStruct : BaseStruct
    {
        public MessageContentStruct(BaseStruct parentStruct) : base(parentStruct)
        {

        }

        public List<AttachmentStruct> AttachmentProperties = new List<AttachmentStruct>();
        public List<RecipientStruct> RecipientProperties = new List<RecipientStruct>();

        protected override string Name
        {
            get
            {
                return "MessageContent";
            }
        }

        internal override IStorage Storage
        {
            get
            {
                return ParentStruct.Storage;
            }
            set
            {
                throw new InvalidProgramException();
            }
        }

        public AttachmentStruct CreateAttachment()
        {
            var attachmentStruct = new AttachmentStruct(this, AttachmentProperties.Count);
            AttachmentProperties.Add(attachmentStruct);
            return attachmentStruct;
        }

        public RecipientStruct CreateRecipient()
        {
            var recipientStruct = new RecipientStruct(this, RecipientProperties.Count);
            RecipientProperties.Add(recipientStruct);
            return recipientStruct;
        }

        protected override void BuildHeader(IStream propertyStream)
        {
            // 1.1.1 Set 8 bytes reserve.
            propertyStream.WriteZero(8);
            // 1.1.2 Set Next Recipient Id
            Int32 recipientCount = RecipientProperties.Count;
            propertyStream.Write(recipientCount);
            // 1.1.3 Set Next Attachment Id
            propertyStream.Write(AttachmentProperties.Count);
            // 1.1.4 Set Recipient Count
            propertyStream.Write(recipientCount);
            // 1.1.5 Set Attachment Count
            propertyStream.Write(AttachmentProperties.Count);
        }

        protected override void BuildEx()
        {
            // it can call any code. need be called in parent struct, like TopLevelStruct or EmbedStruct.
        }

        internal void BuildInternal()
        {
            if (RecipientProperties.Count == 0 && AttachmentProperties.Count == 0 && Properties.Properties.Count == 0)
                return;

            if (this.Properties.Properties.Count > 0)
            {
                this.Properties.AddProperty(new SpecialFixProperty(0x340D0003, BitConverter.GetBytes(0x00040E79)));
                this.Properties.AddProperty(new SpecialFixProperty(0x0FF40003, BitConverter.GetBytes((uint)0x02)));

                if (this.Properties.ContainProperty(0x10090102))
                    this.Properties.AddProperty(new SpecialFixProperty(0x0E1F000B, BitConverter.GetBytes(true)));
                else
                    this.Properties.AddProperty(new SpecialFixProperty(0x0E1F000B, BitConverter.GetBytes(false)));

                if (this.AttachmentProperties.Count > 0)
                {
                    this.Properties.AddProperty(new SpecialFixProperty(0x0E1B000B, BitConverter.GetBytes(true)));
                }
                else
                {
                    this.Properties.AddProperty(new SpecialFixProperty(0x0E1B000B, BitConverter.GetBytes(false)));
                }

                this.Properties.AddProperty(new SpecialVarStringProperty(0x0E02001F, new byte[0]));

                byte[] ccDisplayName = GetDisplayName(RecvType.CC);
                byte[] toDisplayName = GetDisplayName(RecvType.To);

                this.Properties.AddProperty(new SpecialVarStringProperty(0x0E03001F, ccDisplayName));
                this.Properties.AddProperty(new SpecialVarStringProperty(0x0E04001F, toDisplayName));

            }
            base.BuildEx();


        }

        private byte[] GetDisplayName(RecvType type)
        {
            if (RecipientProperties.Count == 0)
                return new byte[0];
            List<byte> result = new List<byte>();
            
            foreach(var recvStruct in RecipientProperties)
            {
                var recvTypeProp = recvStruct.Properties.GetProperty(0x0C150003);
                var recvType = BitConverter.ToInt32(recvTypeProp.PropValue.BytesForMsg, 0);
                if(recvType == (int)type)
                {
                    IPropValue recvDisplayName = null;
                    if (recvStruct.Properties.ContainProperty(0x3001001F))
                    {
                        recvDisplayName = recvStruct.Properties.GetProperty(0x3001001F);
                    }
                    else if (recvStruct.Properties.ContainProperty(0x3A20001F))
                    {
                        recvDisplayName = recvStruct.Properties.GetProperty(0x3A20001F);
                    }
                    else
                        throw new InvalidProgramException();

                    result.AddRange(recvDisplayName.PropValue.BytesForMsg);
                    result.AddRange(BitConverter.GetBytes((short)0x003B));
                }
            }

            if(result.Count > 0)
            {
                result.RemoveAt(result.Count - 1);
                result.RemoveAt(result.Count - 1);
            }

            return result.ToArray();
        }

        enum RecvType : byte
        {
            To = 1,
            CC = 2,
            BCC = 3,
        }

        protected override byte[] ModifyPropertyValue(IPropValue property)
        {
            if (property.PropTag.PropertyId == 0x0FF7)
            {
                return BitConverter.GetBytes((int)0x00);
            }
            return base.ModifyPropertyValue(property);
        }

        protected override void Release(bool hasError)
        {
        }
    }
}
