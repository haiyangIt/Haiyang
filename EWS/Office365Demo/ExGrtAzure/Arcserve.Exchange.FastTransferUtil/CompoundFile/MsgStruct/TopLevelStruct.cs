using Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct.Helper;
using Arcserve.Exchange.FastTransferUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct
{
    public class TopLevelStruct : MessageContentStruct
    {
        protected override string Name
        {
            get
            {
                return "TopLevelStruct";
            }
        }

        private IStorage rootStorage;
        internal override IStorage Storage
        {
            get; set;
        }

        private Dictionary<int, IPropValue> _namedProperties = new Dictionary<int, IPropValue>();
        protected override Dictionary<int, IPropValue> NamedProperties
        {
            get
            {
                return _namedProperties;
            }
        }

        private Dictionary<uint, uint> _namedPropIdToChangedId = new Dictionary<uint, uint>();
        protected override Dictionary<uint, uint> NamedPropIdToChangedId
        {
            get
            {
                return _namedPropIdToChangedId;
            }
        }

        public TopLevelStruct(IStorage rootStorage) : base(null)
        {
            this.rootStorage = rootStorage;
            Storage = rootStorage;
        }

        private static Guid PS_MAPI_CA = new Guid(0x00020328, 0x0000, 0x0000, 0xC0, 0x00, 0x0, 0x00, 0x0, 0x00, 0x00, 0x46);
        private static Guid PS_PUBLIC_STRINGS_CA = new Guid(0x00020329, 0x0000, 0x0000, 0xC0, 0x00, 0x0, 0x00, 0x0, 0x00, 0x00, 0x46);

        #region Build Struct
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

        protected override void BuildEx()
        {
            BuildNamedProperty();
            BuildInternal();
        }

        private void BuildNamedProperty()
        {
            //1. Create NameidStorage
            IStorage m_pStorage = null;
            IStream m_pGuidStream = null;
            IStream m_pEntryStream = null;
            IStream m_pStringStream = null;
            Dictionary<uint, IStream> m_mapStream = new Dictionary<uint, IStream>();

            try
            {
                m_pStorage = CompoundFileUtil.Instance.GetChildStorage("__nameid_version1.0", true, Storage);

                //2. Create GUID Stream
                m_pGuidStream = CompoundFileUtil.Instance.GetChildStream("__substg1.0_00020102", true, m_pStorage);

                //3. Create Entry Stream
                m_pEntryStream = CompoundFileUtil.Instance.GetChildStream("__substg1.0_00030102", true, m_pStorage);

                //4. Create String Stream
                m_pStringStream = CompoundFileUtil.Instance.GetChildStream("__substg1.0_00040102", true, m_pStorage);

                uint index = 0;
                uint size = (uint)NamedProperties.Count;
                Dictionary<Guid, uint> m_guidIndex = new Dictionary<Guid, uint>();
                const uint _GuidStartIndex = 3;
                uint m_stringStreamWritedCount = 0;

                foreach (var keyValue in NamedProperties)
                {
                    var propValue = keyValue.Value;

                    //1. if guid is not exist, add value to guid stream and guid map
                    var namedPropInfo = propValue.PropInfo as NamePropInfo;
                    Guid itemGuid = namedPropInfo.GetNamedGuid();
                    //GUID itemGuid = *(propertyItem.namedID.lpguid);
                    uint guidIndex = 0;

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
                        uint findIndex = 0;
                        if (m_guidIndex.TryGetValue(itemGuid, out findIndex))
                        {
                            guidIndex = _GuidStartIndex + findIndex;
                        }
                        else
                        {
                            guidIndex = 3 + (uint)m_guidIndex.Count;
                            var namedGuid = namedPropInfo.GetNamedGuid();
                            m_guidIndex.Add(namedGuid, (uint)m_guidIndex.Count);
                            m_pGuidStream.Write(namedGuid);
                        }
                    }

                    //2. write entry stream
                    uint idOrOffsetPart = 0;
                    uint indexKindPart = 0;
                    uint kind = X00Or01.X00ForDispId;
                    if (namedPropInfo.IsMNID())
                    {
                        idOrOffsetPart = namedPropInfo.GetMNID();
                    }
                    else
                    {
                        idOrOffsetPart = m_stringStreamWritedCount;
                        kind = X00Or01.X01ForName;
                    }

                    m_pEntryStream.Write(idOrOffsetPart);

                    indexKindPart = ((uint)index << 16) | ((uint)(guidIndex << 1)) | kind;
                    m_pEntryStream.Write(indexKindPart);

                    //3. write string stream
                    byte[] name;
                    if (kind == X00Or01.X01ForName)
                    {
                        name = namedPropInfo.GetPropName();
                        uint nameSizeWithoutTerminate = (uint)name.Length - 2;
                        m_pStringStream.Write(nameSizeWithoutTerminate);
                        m_stringStreamWritedCount += 4;

                        m_pStringStream.Write(name, (int)nameSizeWithoutTerminate, IntPtr.Zero);
                        m_stringStreamWritedCount += nameSizeWithoutTerminate;

                        uint leftNeedWriteTerminate = m_stringStreamWritedCount % 4;
                        if (leftNeedWriteTerminate != 0 && index != size - 1)
                        {
                            leftNeedWriteTerminate = 4 - leftNeedWriteTerminate;
                            m_pStringStream.WriteZero((int)leftNeedWriteTerminate);
                            m_stringStreamWritedCount += leftNeedWriteTerminate;
                        }
                    }

                    //4. Name to Id Stream
                    uint id = 0;
                    if (kind == X00Or01.X00ForDispId)
                    {
                        id = namedPropInfo.GetMNID();
                    }
                    else
                    {
                        id = namedPropInfo.GetCrc32FromName();
                    }
                    uint streamId = 0x1000 + ((id ^ ((guidIndex << 1) | kind)) % 0x1F);
                    uint nStreamName = (streamId << 16) | 0x0102;

                    IStream pStream = null;
                    if (!m_mapStream.TryGetValue(nStreamName, out pStream))
                    {
                        string streamName = string.Format("__substg1.0_{0:X8}", nStreamName);
                        pStream = CompoundFileUtil.Instance.GetChildStream(streamName, true, m_pStorage);

                        m_mapStream.Add(nStreamName, pStream);
                    }

                    pStream.Write(id);
                    pStream.Write(indexKindPart);

                    index++;
                }


                m_pGuidStream.Commit(0);
                m_pEntryStream.Commit(0);
                m_pStringStream.Commit(0);
                foreach (var keyValue in m_mapStream)
                {
                    keyValue.Value.Commit(0);
                }

                m_pStorage.Commit(0);
            }

            finally
            {
                CompoundFileUtil.Instance.ReleaseComObj(m_pGuidStream);
                CompoundFileUtil.Instance.ReleaseComObj(m_pEntryStream);
                CompoundFileUtil.Instance.ReleaseComObj(m_pStringStream);



                foreach (var keyValue in m_mapStream)
                {
                    var stream = keyValue.Value;
                    CompoundFileUtil.Instance.ReleaseComObj(stream);
                }

                CompoundFileUtil.Instance.ReleaseComObj(m_pStorage);

            }
        }
        #endregion

        #region Parser 
        protected override void ParserHeader(IStream propertyHeaderStream, ref int readCount)
        {
            base.ParserHeader(propertyHeaderStream, ref readCount);
            propertyHeaderStream.ReadInt64(ref readCount);
        }

        protected override void GetStorageForParser()
        {
            Storage = rootStorage;
        }

        public override void Parser()
        {
            ParserInternal();
        }

        #endregion


    }
}
