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
            // 1.1.6 Set 8 bytes Reserve;
            propertyStream.WriteZero(8);
        }

        public override void Build()
        {
            BuildNamedProperty();
            base.Build();
        }

        private void BuildNamedProperty()
        {
            //1. Create NameidStorage
            IStorage m_pStorage = null;
            IStream m_pGuidStream = null;
            IStream m_pEntryStream = null;
            IStream m_pStringStream = null;
            m_pStorage = CompoundFileUtil.Instance.GetChildStorage("__nameid_version1.0", true, Storage);
            
            //2. Create GUID Stream
            m_pGuidStream = CompoundFileUtil.Instance.GetChildStream("__substg1.0_00020102", true, m_pStorage);

            //3. Create Entry Stream
            m_pEntryStream = CompoundFileUtil.Instance.GetChildStream("__substg1.0_00030102", true, m_pStorage);

            //4. Create String Stream
            m_pStringStream = CompoundFileUtil.Instance.GetChildStream("__substg1.0_00040102", true, m_pStorage);



            int index = 0;
            int size = NamedProperties.Count;

            foreach(var keyValue in NamedProperties)
            {
                var propValue = keyValue.Value;

                //1. if guid is not exist, add value to guid stream and guid map
                var namedPropInfo = propValue.PropInfo as NamePropInfo;
                Guid itemGuid = namedPropInfo.GetNamedGuid();
                //GUID itemGuid = *(propertyItem.namedID.lpguid);
                UINT16 guidIndex = 0;

                if (itemGuid == PS_MAPI_CA)
                {
                    guidIndex = 1;
                }
                else if (itemGuid == PS_PUBLIC_STRINGS_CA)
                {
                    guidIndex = 2;
                }
                else
                {
                    auto findResult = m_guidIndex.find(itemGuid);
                    if (findResult == m_guidIndex.end())
                    {
                        guidIndex = 3 + m_guidIndex.size();
                        m_guidIndex.insert(make_pair(*(propertyItem.namedID.lpguid), m_guidIndex.size()));
                        hr = m_pGuidStream->Write(propertyItem.namedID.lpguid, sizeof(GUID), NULL);
                        if (hr)
                        {
                            TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, write property [%08x] guid stream failed with :%08x.", propertyItem.nTag, hr);
                            break;
                        }
                    }
                    else
                    {
                        guidIndex = 3 + findResult->second;
                    }

                }

                //2. write entry stream
                ULONG idOrOffsetPart = 0;
                ULONG indexKindPart = 0;
                if (propertyItem.namedID.ulKind == MNID_ID)
                {
                    idOrOffsetPart = propertyItem.namedID.Kind.lID;
                }
                else
                {
                    idOrOffsetPart = m_stringStreamWritedCount;
                }
                hr = m_pEntryStream->Write(&idOrOffsetPart, 4, NULL);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, write property [%08x] id or string offset failed with :%08x.", propertyItem.nTag, hr);
                    break;
                }

                indexKindPart = ((UINT32)index << 16) | ((UINT32)(guidIndex << 1)) | propertyItem.namedID.ulKind;
                hr = m_pEntryStream->Write(&indexKindPart, 4, NULL);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, write property [%08x] index and kind failed with :%08x.", propertyItem.nTag, hr);
                    break;
                }

                //3. write string stream
                if (propertyItem.namedID.ulKind == MNID_STRING)
                {
                    name = propertyItem.namedID.Kind.lpwstrName;
                    ULONG nameSizeWithoutTerminate = name.size() * sizeof(wchar_t);
                    hr = m_pStringStream->Write(&nameSizeWithoutTerminate, 4, NULL);
                    if (hr)
                    {
                        TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, write property [%08x] string length failed with :%08x.", propertyItem.nTag, hr);
                        break;
                    }
                    m_stringStreamWritedCount += 4;

                    hr = m_pStringStream->Write(name.c_str(), nameSizeWithoutTerminate, NULL);
                    if (hr)
                    {
                        TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, write property [%08x] string data failed with :%08x.", propertyItem.nTag, hr);
                        break;
                    }
                    m_stringStreamWritedCount += nameSizeWithoutTerminate;

                    int leftNeedWriteTerminate = m_stringStreamWritedCount % 4;
                    if (leftNeedWriteTerminate != 0 && index != size - 1)
                    {
                        leftNeedWriteTerminate = 4 - leftNeedWriteTerminate;
                        hr = m_pStringStream->Write(&zero, leftNeedWriteTerminate, NULL);
                        if (hr)
                        {
                            TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, write property [%08x] zero data failed with :%08x.", propertyItem.nTag, hr);
                            break;
                        }
                        m_stringStreamWritedCount += leftNeedWriteTerminate;
                    }
                }

                //4. Name to Id Stream
                ULONG32 id = 0;
                if (propertyItem.namedID.ulKind == MNID_ID)
                {
                    id = propertyItem.namedID.Kind.lID;
                }
                else
                {
                    hr = CCrc32Helper::GetCrc32(name, id);
                    if (hr)
                    {
                        TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, get property [%08x] string crc32 data failed with :%08x.", propertyItem.nTag, hr);
                        break;
                    }
                }
                ULONG32 streamId = 0x1000 + (id ^ ((guidIndex << 1) | propertyItem.namedID.ulKind)) % 0x1F;
                ULONG32 nStreamName = (streamId << 16) | 0x0102;

                auto findStream = m_mapStream.find(nStreamName);
                IStream* pStream = NULL;
                if (findStream == m_mapStream.end())
                {
                    wchar_t streamName[SizeNameToIdMappingStream];
                    swprintf_s(streamName, NameNameToIdMappingStream, nStreamName);
                    HRESULT hr = m_pStorage->CreateStream(streamName, STGM_CREATE | STGM_SHARE_EXCLUSIVE | STGM_READWRITE, 0, 0, &pStream);
                    if (hr)
                    {
                        TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, create property [%08x] stream failed with :%08x.", propertyItem.nTag, hr);
                        break;
                    }

                    m_mapStream.insert(make_pair(nStreamName, pStream));
                }
                else
                {
                    pStream = findStream->second;
                }

                hr = pStream->Write(&id, 4, NULL);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, write property [%08x] id into property stream failed with :%08x.", propertyItem.nTag, hr);
                    break;
                }

                hr = pStream->Write(&indexKindPart, 4, NULL);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, write property [%08x] index&kind into property stream failed with :%08x.", propertyItem.nTag, hr);
                    break;
                }

            }

            for (auto begin = m_allNamedProperties.begin();
                begin != m_allNamedProperties.end();
                begin++, index++)
            {
                hr = BuildEachProperty(*begin, index, size);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, build property [%08x] failed with :%08x.", begin->nTag, hr);
                    break;
                }
            }

            if (hr)
            {
                break;
            }
            else
            {
                hr = m_pGuidStream->Commit(STGC_DEFAULT);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, Commit property guid stream failed with :%08x.", hr);
                    break;
                }

                hr = m_pEntryStream->Commit(STGC_DEFAULT);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, Commit property guid stream failed with :%08x.", hr);
                    break;
                }

                hr = m_pStringStream->Commit(STGC_DEFAULT);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, Commit property guid stream failed with :%08x.", hr);
                    break;
                }

                for (auto streamBegin = m_mapStream.begin(); streamBegin != m_mapStream.end(); streamBegin++)
                {
                    hr = streamBegin->second->Commit(STGC_DEFAULT);
                    if (hr)
                    {
                        TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, Commit property [%08x] stream failed with :%08x.", streamBegin->first, hr);
                        break;
                    }
                }

                hr = m_pStorage->Commit(STGC_DEFAULT);
                if (hr)
                {
                    TRACE_LOG2(CA_LOG_ERR, L"When build named property mapping, Commit named mapping property storage failed with :%08x.", hr);
                    break;
                }
            }
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

        public virtual void Build()
        {
            BuildProperties();
            var storage = Storage;
            CompoundFileUtil.Instance.ReleaseComObj(ref storage);
        }

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
                if (PropertyTag.IsMultiVarType((ushort)property.PropTag.PropertyTag)) // MVVarValue
                {
                    // write length stream
                    var streamName = string.Format("__substg1.0_{0}", property.PropTag.PropertyTag.ToString("X8"));
                    IStream propStream = null;
                    try
                    {
                        propStream = CompoundFileUtil.Instance.GetChildStream(streamName, true, storage);

                        if (property.PropTag.PropertyTag == (int)PropertyType.PT_MV_BINARY)
                        {
                            foreach (var item in (MvVarSizeValue)property.PropValue)
                            {
                                MvVarSizeItem mvItem = item as MvVarSizeItem;

                                propStream.Write(mvItem.GetValueLength());
                                propStream.WriteZero(4);
                            }
                        }
                        else if (property.PropTag.PropertyTag == (int)PropertyType.PT_MV_STRING8 || ((property.PropTag.PropertyTag & 0x9000) == 0x9000))
                        {
                            foreach (var item in (MvVarSizeValue)property.PropValue)
                            {
                                MvVarSizeItem mvItem = item as MvVarSizeItem;
                                propStream.Write(mvItem.GetValueLength());
                            }
                        }
                        else
                            throw new ArgumentException("Mv propertyTag is wrong.");

                        propStream.Commit(0);
                    }
                    finally
                    {
                        CompoundFileUtil.Instance.ReleaseComObj(ref propStream);
                    }

                    // write value stream
                    int itemIndex = 0;
                    foreach (var item in (MvVarSizeValue)property.PropValue)
                    {
                        MvVarSizeItem mvItem = item as MvVarSizeItem;
                        string valueStreamName = string.Format("__substg1.0_{0}-{1}", property.PropTag.PropertyTag, itemIndex.ToString("X8"));
                        IStream valueStream = null;
                        try
                        {
                            valueStream = CompoundFileUtil.Instance.GetChildStream(valueStreamName, true, Storage);
                            valueStream.Write(mvItem.GetValueBytes());
                            valueStream.Commit(0);
                        }
                        finally
                        {
                            CompoundFileUtil.Instance.ReleaseComObj(ref valueStream);
                        }
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
                propertyStream.WriteZero(4);
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
                propertyStream.WriteZero(4);
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
    }

    public class RecipientStruct : PropertyCollection
    {
        public RecipientStruct(PropertyCollection parentStruct, int index) : base(parentStruct)
        {
            Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(index), true, parentStruct.Storage);
        }

        private string GetStorageName(int index)
        {
            return string.Format("__recip_version1.0_#{0}", index.ToString("X8"));
        }

        protected override void BuildHeader(IStream propertyStream)
        {
            // 1.1.1 Set 8 bytes reserve.
            propertyStream.WriteZero(8);
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
