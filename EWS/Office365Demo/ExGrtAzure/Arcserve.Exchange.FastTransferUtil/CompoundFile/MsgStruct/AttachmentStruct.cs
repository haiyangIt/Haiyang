using Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct.Helper;
using Arcserve.Exchange.FastTransferUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct
{
    public class AttachmentStruct : BaseStruct
    {
        public EmbedStruct Embed;
        private readonly int _attachIndex;

        protected override string Name
        {
            get
            {
                return string.Format("Attachment_{0}", _attachIndex);
            }
        }

        public AttachmentStruct(BaseStruct parentStruct, int attachIndex) : base(parentStruct)
        {
            _attachIndex = attachIndex;
        }

        private string GetStorageName(int index)
        {
            return string.Format("__attach_version1.0_#{0:X8}", index);
        }

        public EmbedStruct CreateEmbedStruct()
        {
            Embed = new EmbedStruct(this);
            return Embed;
        }

        protected override void BuildHeader(IStream propertyStream)
        {
            // 1.1.1 Set 8 bytes reserve.
            propertyStream.WriteZero(8);
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

        protected override byte[] ModifyPropertyValue(IPropValue property)
        {
            if (property.PropTag.PropertyId == 0x0FF9)
            {
                return BitConverter.GetBytes((int)_attachIndex);
            }
            return base.ModifyPropertyValue(property);
        }

        protected override int ModifyPropertyLength(IPropValue property)
        {
            if (property.PropTag.PropertyId == 0x0FF9)
            {
                return 4;
            }
            return base.ModifyPropertyLength(property);
        }

        protected override void CreateSelfStorageForBuild()
        {
            if (Storage == null)
            {
                Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(_attachIndex), true, ParentStruct.Storage);
            }
        }
        protected override void BuildEx()
        {
            if (this.Properties.Properties.Count > 0)
            {
                if (Embed != null && Embed.MessageContent.Properties.Properties.Count > 0)
                {
                    IPropValue attachMethod = this.Properties.GetProperty(0x37050003);
                    int method = 0x05;
                    if (attachMethod != null)
                    {
                        method = BitConverter.ToInt32(attachMethod.PropValue.BytesForMsg, 0);
                    }
                    else
                    {
                        throw new InvalidProgramException();
                    }

                    if (method == 0x05)
                    {
                        this.Properties.AddProperty(new SpecialFixProperty(0x3701000D, BitConverter.GetBytes(0x00000001FFFFFFFF)));
                    }
                    else if (method == 0x06)
                    {
                        this.Properties.AddProperty(new SpecialFixProperty(0x3701000D, BitConverter.GetBytes(0x00000004FFFFFFFF)));
                    }
                    else
                        throw new InvalidProgramException();
                }

                this.Properties.AddProperty(new SpecialFixProperty(0x0E210003, BitConverter.GetBytes(_attachIndex)));
                this.Properties.AddProperty(new SpecialFixProperty(0x340D0003, BitConverter.GetBytes(0x00040E79)));
                this.Properties.AddProperty(new SpecialFixProperty(0x0FFE0003, BitConverter.GetBytes((uint)0x07)));
                this.Properties.AddProperty(new SpecialFixProperty(0x0FF40003, BitConverter.GetBytes((uint)0x02)));

                var prop371Dvalue = this.Properties.GetProperty(0x0FF90102).PropValue.BytesForMsg;
                this.Properties.AddProperty(new SpecialVarBinaryProperty(0x371D0102, prop371Dvalue));
            }

            base.BuildEx();
        }

        protected override void ParserHeader(IStream propertyHeaderStream, ref int readCount)
        {
            propertyHeaderStream.ReadInt64(ref readCount);
        }

        protected override void ParserEx()
        {
            base.ParserEx();
            if (this.Properties.ContainProperty(0x3701000D))
            {
                Embed = new EmbedStruct(this);
                Embed.Parser();
            }
        }

        protected override void GetStorageForParser()
        {
            if(Storage == null)
            {
                Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(_attachIndex), false, ParentStruct.Storage);
            }
        }

        public override bool Compare(object other, StringBuilder result, int indent)
        {
            var desStruct = other as AttachmentStruct;
            if (desStruct == null)
            {
                BaseStruct.SetMessage(result, indent, "{0} Type is not AttachmentStruct.", Name);
                return false;
            }

            bool returnResult = true;

            var isSame = base.Compare(other, result, indent);
            if (!isSame) returnResult = false;

            if(Embed != null)
            {
                if(!Embed.Compare(desStruct.Embed, result, indent + 2))
                {
                    returnResult = false;
                }
            }

            return returnResult;
        }
    }
}
