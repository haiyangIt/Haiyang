using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct.Helper;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.Item.PropValue;

namespace Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct
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

        //internal override IStorage Storage
        //{
        //    get
        //    {
        //        return ParentStruct.Storage;
        //    }
        //    set
        //    {
        //        throw new InvalidProgramException();
        //    }
        //}

        protected override void CreateSelfStorageForBuild()
        {
            Storage = ParentStruct.Storage;
        }

        #region Build Struct
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

                if (this.Properties.ContainProperty(0x3FD9001F))
                {
                    var property3FD9 = Properties.GetProperty(0x3FD9001F);
                    this.Properties.AddProperty(new SpecialVarStringProperty(0x1000001F, property3FD9.PropValue.BytesForMsg));
                }

            }
            base.BuildEx();
        }

        private byte[] GetDisplayName(RecvType type)
        {
            if (RecipientProperties.Count == 0)
                return new byte[0];
            List<byte> result = new List<byte>();

            foreach (var recvStruct in RecipientProperties)
            {
                var recvTypeProp = recvStruct.Properties.GetProperty(0x0C150003);
                var recvType = BitConverter.ToInt32(recvTypeProp.PropValue.BytesForMsg, 0);
                if (recvType == (int)type)
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
                    result.AddRange(BitConverter.GetBytes((short)0x0020));

                }
            }

            if (result.Count > 0)
            {
                result.RemoveAt(result.Count - 1);
                result.RemoveAt(result.Count - 1);
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
        #endregion

        protected override void Release(bool hasError)
        {
        }

        protected override void ParserHeader(IStream propertyHeaderStream, ref int readCount)
        {
            Int64 value = propertyHeaderStream.ReadInt64(ref readCount);
            Int32 recipientCount = propertyHeaderStream.ReadInt32(ref readCount);
            Int32 attachmentCount = propertyHeaderStream.ReadInt32(ref readCount);
            recipientCount = propertyHeaderStream.ReadInt32(ref readCount);
            attachmentCount = propertyHeaderStream.ReadInt32(ref readCount);

            for (int i = 0; i < recipientCount; i++)
            {
                var recipientStruct = CreateRecipient();
                recipientStruct.Parser();
            }

            for (int i = 0; i < attachmentCount; i++)
            {
                var attachmentStruct = CreateAttachment();
                attachmentStruct.Parser();
            }
        }

        protected override void GetStorageForParser()
        {
            if (Storage == null)
            {
                Storage = ParentStruct.Storage;
            }
        }

        protected override void ParserEx()
        {

        }

        internal void ParserInternal()
        {
            base.ParserEx();
        }


        public override bool Compare(object other, StringBuilder result, int indent)
        {
            var desStruct = other as MessageContentStruct;
            if (desStruct == null)
            {
                BaseStruct.SetMessage(result, indent, "{0} Type is not messagecontent.", Name);
                return false;
            }

            bool returnResult = true;

            var isSame = base.Compare(other, result, indent);
            if (!isSame) returnResult = false;

            if (RecipientProperties.Count != desStruct.RecipientProperties.Count)
            {
                BaseStruct.SetMessage(result, indent, "{0} RecipientCount is not same, left: {1}, right: {2}.", Name, RecipientProperties.Count, desStruct.RecipientProperties.Count);
            }
            else
            {
                var count = RecipientProperties.Count;
                for (int i = 0; i < count; i++)
                {
                    if(!RecipientProperties[i].Compare(desStruct.RecipientProperties[i], result, indent + 2))
                    {
                        returnResult = false;
                    }
                }
            }

            if (AttachmentProperties.Count != desStruct.AttachmentProperties.Count)
            {
                BaseStruct.SetMessage(result, indent, "{0} AttachmentCount is not same, left: {1}, right: {2}.", Name, AttachmentProperties.Count, desStruct.AttachmentProperties.Count);
            }
            else
            {
                var count = AttachmentProperties.Count;
                for (int i = 0; i < count; i++)
                {
                    if(!AttachmentProperties[i].Compare(desStruct.AttachmentProperties[i], result, indent + 2))
                    {
                        returnResult = false;
                    }
                }
            }

            return returnResult;
        }
    }
}
