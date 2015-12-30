using FTStreamUtil;
using FTStreamUtil.Item;
using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FastTransferUtil.CompoundFile
{
    public class CompoundFileBuild
    {
        public CompoundFileBuild()
        {

        }

        public IStorage RootStorage;
        private TopLevelStruct _topPropStruct;
        private PropertyCollection _currentPropCollection;
        private Stack<PropertyCollection> PropCollection;

        private PropertyCollection CurrentPropCollection
        {
            get
            {
                if (_currentPropCollection == null)
                {
                    _currentPropCollection = PropCollection.Pop();
                }
                return _currentPropCollection;
            }
            set
            {
                if (_currentPropCollection != null)
                {
                    PropCollection.Push(_currentPropCollection);
                }

                _currentPropCollection = value;
            }
        }

        internal void EndWriteMessageContent()
        {
            CurrentPropCollection.Build();
            _currentPropCollection = null;
        }

        internal void StartWriteMessageContent()
        {
            if (_topPropStruct == null)
            {
                _topPropStruct = new TopLevelStruct(RootStorage);
                CurrentPropCollection = _topPropStruct;
            }
            else
            {
                CurrentPropCollection = ((EmbedStruct)_currentPropCollection).CreateMessageContent();
            }
        }

        internal void AddProperty(IPropValue propValue)
        {
            CurrentPropCollection.AddProperties(propValue);
        }

        internal void EndWriteAttachment()
        {
            CurrentPropCollection.Build();
            _currentPropCollection = null;
        }

        internal void EndWriteEmbed()
        {
            CurrentPropCollection.Build();
            _currentPropCollection = null;
        }

        internal void EndWriteRecipient()
        {
            CurrentPropCollection.Build();
            _currentPropCollection = null;
        }

        internal void SetRootStorage(IStorage storage)
        {
            RootStorage = storage;
        }

        internal void StartWriteAttachment()
        {
            CurrentPropCollection = ((MessageContentStruct)CurrentPropCollection).CreateAttachment();
        }

        internal void StartWriteEmbed()
        {
            CurrentPropCollection = ((AttachmentStruct)CurrentPropCollection).CreateEmbedStruct();
        }

        internal void StartWriteRecipient()
        {
            CurrentPropCollection = ((MessageContentStruct)CurrentPropCollection).CreateRecipient();
        }
    }

    public class TopLevelStruct : MessageContentStruct
    {
        private IStorage rootStorage;

        private Dictionary<int, IPropValue> _namedProperties;
        protected override Dictionary<int, IPropValue> NamedProperties
        {
            get
            {
                return _namedProperties;
            }
        }

        public TopLevelStruct(IStorage rootStorage) : base(null)
        {
            this.rootStorage = rootStorage;
            Storage = rootStorage;
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }

        protected override void BuildHeader(IStream propertyStream)
        {

            // 1.1.1 Set 8 bytes reserve.
            propertyStream.Write((Int32)0);
            // 1.1.2 Set Next Recipient Id
            Int32 recipientCount = RecipientProperties.Count;
            propertyStream.Write(recipientCount);
            // 1.1.3 Set Next Attachment Id
            propertyStream.Write(AttachmentProperties.Count);
            // 1.1.4 Set Recipient Count
            propertyStream.Write(recipientCount);
            // 1.1.5 Set Attachment Count
            propertyStream.Write(AttachmentProperties.Count);
            // 1.1.6 Set 8 bytes Reserve;
            propertyStream.Write((Int32)0);
        }
    }

    public class MessageContentStruct : PropertyCollection
    {
        public MessageContentStruct(PropertyCollection parentStruct) : base(parentStruct)
        {

        }

        public List<AttachmentStruct> AttachmentProperties = new List<AttachmentStruct>();
        public List<RecipientStruct> RecipientProperties = new List<RecipientStruct>();
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

        public override void Build()
        {

        }
    }

    public abstract class PropertyCollection
    {
        protected PropertyCollection(PropertyCollection parentStruct)
        {
            ParentStruct = parentStruct;
        }

        public Props Properties;

        protected virtual Dictionary<int, IPropValue> NamedProperties
        {
            get
            {
                return this.ParentStruct.NamedProperties;
            }
        }

        public virtual void AddProperties(IPropValue property)
        {
            Properties.AddProperty(property);
            if (property.PropInfo is NamePropInfo)
            {
                NamedProperties[property.PropTag.PropertyId] = property;
            }
        }

        public abstract void Build();

        protected PropertyCollection ParentStruct
        {
            get; set;
        }

        internal virtual IStorage Storage
        {
            get; set;
        }

        protected virtual void BuildProperties()
        {
            IStream propertyStream = CreatePropertyStream();
            BuildHeader(propertyStream);
            foreach (IPropValue property in Properties.Properties)
            {
                BuildEachProperty(Storage, propertyStream, property);
            }
        }

        protected virtual void BuildEachProperty(IStorage storage, IStream propertyStream, IPropValue property)
        {
            WritePropertyInfoToPropertyStream(propertyStream, property);
            WritePropertyValueToStorage(storage, property);
        }

        private void WritePropertyValueToStorage(IStorage storage, IPropValue property)
        {
            if (property is VarPropValue)
            {
                var streamName = string.Format("__substg1.0_{0}", property.PropTag.PropertyTag.ToString("X8"));
                IStream varPropStream = null;
                try
                {
                    varPropStream = CompoundFileUtil.Instance.GetChildStream(streamName, true, storage);
                    varPropStream.Write(property.PropValue.Bytes);
                    varPropStream.Commit(0);
                }
                finally
                {
                    CompoundFileUtil.Instance.ReleaseComObj(ref varPropStream);
                }
            }
            else if (property is MvPropValue)
            {
                if (PropertyTag.IsMultiVarType((ushort)property.PropTag.PropertyTag))
                {
                    var streamName = string.Format("__substg1.0_{0}", property.PropTag.PropertyTag.ToString("X8"));
                    IStream propStream = null;
                    try
                    {
                        propStream = CompoundFileUtil.Instance.GetChildStream(streamName, true, storage);
                        propStream.Write(((MvVarSizeValue)property.PropValue).GetItemLengthByte());
                        if (property.PropTag.PropertyTag == (int)PropertyType.PT_MV_BINARY)
                        {
                            propStream.WriteZero(4);
                        }
                        propStream.Commit(0);
                    }
                    finally
                    {
                        CompoundFileUtil.Instance.ReleaseComObj(ref propStream);
                    }


                }
                else
                {
                    var streamName = string.Format("__substg1.0_{0}", property.PropTag.PropertyTag.ToString("X8"));
                    IStream propStream = null;
                    try
                    {
                        propStream = CompoundFileUtil.Instance.GetChildStream(streamName, true, storage);
                        propStream.Write(((MvFixedSizeValue)property.PropValue).GetItemValueByte());
                        propStream.Commit(0);
                    }
                    finally
                    {
                        CompoundFileUtil.Instance.ReleaseComObj(ref propStream);
                    }
                }
            }
        }

        protected virtual void WritePropertyInfoToPropertyStream(IStream propertyStream, IPropValue property)
        {
            if (property is FixPropValue)
            {
                // 1.2.1 Set fixed data
                // 1.2.1.1 Set Tag
                propertyStream.Write(property.PropTag.Bytes);
                // 1.2.1.2 Set Flag
                propertyStream.Write((Int32)0x06);
                // 1.2.1.3 Set Value
                var value = property.PropValue.Bytes;
                propertyStream.Write(value);
                propertyStream.WriteZero(8 - value.Length);
            }
            else if (property is VarPropValue)
            {
                // 1.2.2 Set variable data
                // 1.2.2.1 Set Tag
                propertyStream.Write(property.PropTag.Bytes);
                // 1.2.2.2 Set Flag
                propertyStream.Write((Int32)0x06);
                // 1.2.2.3 Set Size
                var valueSize = property.PropValue.BytesCount;
                if (property.PropValue is VarStringValue)
                {

                    if (valueSize == 0)
                        propertyStream.Write((Int32)2);
                    else
                        propertyStream.Write(valueSize);
                }
                else
                {
                    propertyStream.Write(valueSize);
                }
                // 1.2.2.4 Set Reserve , todo if contain embed message, it must set 0x01, if OLE, set 0x04;
                propertyStream.WriteZero(0);
            }
            else if (property is MvPropValue)
            {
                // 1.2.3 Set variable data
                // 1.2.3.1 Set Tag
                propertyStream.Write(property.PropTag.Bytes);
                // 1.2.3.2 Set Flag
                propertyStream.Write((Int32)0x06);
                // 1.2.3.3 Set Size, todo must check the length is all value's bytes total length;
                if (PropertyTag.IsMultiVarType((ushort)property.PropTag.PropertyType))
                {
                    propertyStream.Write((Int32)(((ISizeValue)property.PropValue).ItemCount) * 4);
                }
                else
                {
                    propertyStream.Write((Int32)(((ISizeValue)property.PropValue).ItemCount) * PropertyTag.GetFixPropertyTypeLength((ushort)property.PropTag.PropertyType));
                }
                // 1.2.3.4 Set Reserve , todo if contain embed message, it must set 0x01, if OLE, set 0x04;
                propertyStream.WriteZero(0);
            }
        }

        protected virtual void BuildHeader(IStream propertyStream)
        {
            throw new NotImplementedException();
        }

        protected virtual IStream CreatePropertyStream()
        {
            return CompoundFileUtil.Instance.GetChildStream("__properties_version1.0", true, Storage);
        }
    }

    public class Props
    {
        public List<IPropValue> Properties = new List<IPropValue>();
        public void AddProperty(IPropValue property)
        {
            Properties.Add(property);
        }
    }

    public class AttachmentStruct : PropertyCollection
    {
        public EmbedStruct Embed;

        public AttachmentStruct(PropertyCollection parentStruct, int attachIndex) : base(parentStruct)
        {
            Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(attachIndex), true, parentStruct.Storage);
        }

        private string GetStorageName(int index)
        {
            return string.Format("__attach_version1.0_#{0}", index.ToString("X8"));
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }

        public EmbedStruct CreateEmbedStruct()
        {
            Embed = new EmbedStruct(this);
            return Embed;
        }
    }

    public class RecipientStruct : PropertyCollection
    {
        public RecipientStruct(PropertyCollection parentStruct, int index) : base(parentStruct)
        {
            Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(index), true, parentStruct.Storage);
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }

        private string GetStorageName(int index)
        {
            return string.Format("__recip_version1.0_#{0}", index.ToString("X8"));
        }
    }

    public class EmbedStruct : PropertyCollection
    {
        public MessageContentStruct MessageContent;

        public EmbedStruct(PropertyCollection parentStruct) : base(parentStruct)
        {
            Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(), true, parentStruct.Storage);
        }

        private string GetStorageName()
        {
            return "__substg1.0_3701000D";
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }

        public MessageContentStruct CreateMessageContent()
        {
            MessageContent = new MessageContentStruct(this);
            MessageContent.Storage = Storage;
            return MessageContent;
        }
    }

    public static class StreamWriter
    {
        public static void Write(this IStream stream, Int32 value)
        {
            stream.Write(BitConverter.GetBytes(value), sizeof(Int32), IntPtr.Zero);
        }

        public static void Write(this IStream stream, byte[] value)
        {
            stream.Write(value, value.Length, IntPtr.Zero);
        }

        static byte[] zero;
        static byte[] Zero
        {
            get
            {
                if (zero == null)
                {
                    zero = new byte[16];
                    for (int i = 0; i < 16; i++)
                    {
                        zero[i] = 0;
                    }
                }
                return zero;
            }
        }
        public static void WriteZero(this IStream stream, int length)
        {
            stream.Write(Zero, length, IntPtr.Zero);
        }
    }
}
