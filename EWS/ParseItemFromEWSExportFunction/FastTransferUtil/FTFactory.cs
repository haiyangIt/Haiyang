using FTStreamUtil.FTStream;
using FTStreamUtil.Item;
using FTStreamUtil.Item.Marker;
using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FTStreamUtil
{
    public class FTFactory
    {
        private FTFactory()
        {
            //TypeMapping = new Dictionary<MAPIPropertyType, PropertyType>();
            //TypeMapping.Add(MAPIPropertyType.BOOL, PropertyType.PT_BOOLEAN);
            //TypeMapping.Add(MAPIPropertyType.ByteArray, PropertyType.PT_BINARY);
            //TypeMapping.Add(MAPIPropertyType.DateTime, PropertyType.PT_SYSTIME);
            //TypeMapping.Add(MAPIPropertyType.Double, PropertyType.PT_BINARY);
            //TypeMapping.Add(MAPIPropertyType.Int16, PropertyType.PT_BINARY);
            //TypeMapping.Add(MAPIPropertyType.Int32, PropertyType.PT_BINARY);
            //TypeMapping.Add(MAPIPropertyType.Int64, PropertyType.PT_BINARY);
            //TypeMapping.Add(MAPIPropertyType.MVString, PropertyType.PT_BINARY);
            //TypeMapping.Add(MAPIPropertyType.String, PropertyType.PT_BINARY);
        }


        private static object _lock = new object();
        private static FTFactory _instance;
        public static FTFactory Instance
        {
            get
            {
                if (_instance == null)
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new FTFactory();
                        }
                    }
                return _instance;
            }
        }

        public MessageContent CreateMessageContent()
        {
            return new MessageContent();
        }

        public PropList CreatePropList()
        {
            return new PropList();
        }

        public MessageChildren CreateMessageChildren()
        {
            return new MessageChildren();
        }

        public PropertyTag CreatePropertyTag(uint propTag)
        {
            return new PropertyTag(propTag);
        }

        public IPropValue CreatePropValue(PropertyTag propertyTag)
        {
            if (PropertyTag.IsFixedType(propertyTag))
            {
                return new FixPropValue(propertyTag);
            }
            else if (PropertyTag.IsVarType(propertyTag))
            {
                return new VarPropValue(propertyTag);
            }
            else if (PropertyTag.IsMultiType(propertyTag))
            {
                return new MvPropValue(propertyTag);
            }
            else
            {
                throw new ArgumentException(string.Format("Don't support this propertyTag:[{0}].", propertyTag.Data));
            }
        }

        public RecipientCollection CreateRecipientCollection()
        {
            return new RecipientCollection();
        }

        public AttachmentCollection CreateAttachmentCollection()
        {
            return new AttachmentCollection();
        }

        public MetaPropertyFxDelTag CreateFxDelMetaProperty()
        {
            return new MetaPropertyFxDelTag();
        }

        public Recipient CreateRecipient(PropertyTag propertyTag)
        {
            return new Recipient();
        }

        internal Attachment CreateAttachment(PropertyTag propertyTag)
        {
            return new Attachment();
        }

        internal NewAttachmentMarker CreateNewAttachmentMarker()
        {
            return new NewAttachmentMarker();
        }

        internal AttachmentNumberTag CreateAttachmentNumber()
        {
            return new AttachmentNumberTag();
        }

        internal AttachmentContent CreateAttachmentContent()
        {
            return new AttachmentContent();
        }

        internal EndAttachMarker CreateEndAttachmentMarker()
        {
            return new EndAttachMarker();
        }

        internal EmbeddedMessage CreateEmbeddedMessage()
        {
            return new EmbeddedMessage();
        }

        internal StartEmbedMarker CreateStartEmbedMarker()
        {
            return new StartEmbedMarker();
        }

        internal EndEmbedMarker CreateEndEmbedMarker()
        {
            return new EndEmbedMarker();
        }

        internal StartRecipientMarker CreateStartRecipientMarker()
        {
            return new StartRecipientMarker();
        }

        internal EndRecipientMarker CreateEndRecipientMarker()
        {
            return new EndRecipientMarker();
        }

        internal IFixedSizeValue CreateFixedSizeValue(FixedPropType propType, UInt16 propTypeInt)
        {
            switch (propTypeInt)
            {
                case (UInt16)PropertyType.PT_I2:
                    return new FixedInt16Value(propType);
                case (UInt16)PropertyType.PT_LONG:
                    return new FixedInt32Value(propType);
                case (UInt16)PropertyType.PT_R4:
                    return new FixedFloatValue(propType);
                case (UInt16)PropertyType.PT_DOUBLE:
                    return new FixedDoubleValue(propType);
                case (UInt16)PropertyType.PT_CURRENCY:
                    return new FixedCurrencyValue(propType);
                case (UInt16)PropertyType.PT_APPTIME:
                    return new FixedFloatTimeValue(propType);
                case (UInt16)PropertyType.PT_BOOLEAN:
                    return new FixedBooleanValue(propType);
                case (UInt16)PropertyType.PT_I8:
                    return new FixedInt64Value(propType);
                case (UInt16)PropertyType.PT_SYSTIME:
                    return new FixedSysTimeValue(propType);
                case (UInt16)PropertyType.PT_CLSID:
                    return new FixedGuidValue(propType);
                default:
                    throw new ArgumentException(string.Format("The type {0} is not the fixed type.", propType.Data.ToString("X4")));
            }
        }

        internal IPropInfo CreatePropInfo(PropertyTag propertyTag)
        {
            var propId = propertyTag.PropId;
            if (propId < 0x8000)
            {
                return new TagPropId();
            }
            else
            {
                return new NamePropInfo();
            }
        }

        internal FixedPropType CreateFixedPropType()
        {
            return new FixedPropType();
        }

        internal VarPropType CreateVarPropType()
        {
            return new VarPropType();
        }

        internal IVarSizeValue CreateVarSizeValue(VarPropType propType, PropValueLength propValueLength, UInt16 varPropTypeIntValue)
        {
            switch (varPropTypeIntValue)
            {
                case (UInt16)PropertyType.PT_BINARY:
                    return new VarBinaryValue(propType, propValueLength);
                case (UInt16)PropertyType.PT_SERVERID:
                    return new VarServerIdValue(propType, propValueLength);
                case (UInt16)PropertyType.PT_STRING8:
                    return new VarString8Value(propType, propValueLength);
                case (UInt16)PropertyType.PT_OBJECT:
                    return new VarObjectValue(propType, propValueLength);
                default:
                    if ((varPropTypeIntValue & 0x8000) == 0x8000)
                    {
                        return new VarStringValue(propType, propValueLength);
                    }
                    throw new ArgumentException(string.Format("Vartype data {0} is wrong.", propType.Data.ToString("X4")));
            }
        }

        internal PropValueLength CreatePropValueLength()
        {
            return new PropValueLength();
        }

        internal NamedPropId CreateNamedPropId()
        {
            return new NamedPropId();
        }

        internal PropertySet CreatePropertySet()
        {
            return new PropertySet();
        }

        internal X00Or01 CreateX00Or01()
        {
            return new X00Or01();
        }

        internal IDispIdOrName CreateDispIdOrName(X00Or01 _x00Or01)
        {
            if (_x00Or01.Data == X00Or01.X00ForDispId)
                return new DispId();
            else if (_x00Or01.Data == X00Or01.X01ForName)
                return new Name();
            else
                throw new ArgumentException(string.Format("X00or01 data {0} is wrong.", _x00Or01.Data));
        }

        internal IDispIdOrName CreateDispIdOrNameBase(X00Or01 x00Or01)
        {
            return new DispIdOrNameBase(x00Or01);
        }


        //internal IPropValue CreateTransferUnit(byte[] buffer)
        //{
        //    FTTransferUnitBuild unitBuild = new FTTransferUnitBuild(buffer);
        //    return unitBuild.Create();
        //}

        //class FTTransferUnitBuild : IDisposable
        //{
        //    private IFTStreamReader _reader;
        //    private IPropValue _unit;
        //    public FTTransferUnitBuild(byte[] buffer)
        //    {
        //        _reader = new FTStreamReader(buffer);
        //    }

        //    public IPropValue Create()
        //    {
        //        FTStreamParseContext.Instance.SetNewParse(_reader, CreateTransferUnit);
        //        return _unit;
        //    }

        //    private void CreateTransferUnit()
        //    {
        //        var propertyTag = FTStreamParseContext.Instance.ReadPropertyTag();
        //        _unit = FTFactory.Instance.CreatePropValue(propertyTag);
        //    }

        //    public void Dispose()
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        internal MvPropType CreateMvPropType()
        {
            return new MvPropType();
        }

        internal ISizeValue CreateSizeValue(PropertyTag propertyTag, PropValueLength propValueLength)
        {
            ushort baseType = (ushort)(propertyTag.PropType & 0x00FF);
            switch (baseType)
            {
                case (UInt16)PropertyType.PT_I2:
                case (UInt16)PropertyType.PT_LONG:
                case (UInt16)PropertyType.PT_R4:
                case (UInt16)PropertyType.PT_DOUBLE:
                case (UInt16)PropertyType.PT_CURRENCY:
                case (UInt16)PropertyType.PT_APPTIME:
                case (UInt16)PropertyType.PT_BOOLEAN:
                case (UInt16)PropertyType.PT_I8:
                case (UInt16)PropertyType.PT_SYSTIME:
                case (UInt16)PropertyType.PT_CLSID:
                    return new MvFixedSizeValue(propertyTag, propValueLength);

                case (UInt16)PropertyType.PT_BINARY:
                case (UInt16)PropertyType.PT_SERVERID:
                case (UInt16)PropertyType.PT_STRING8:
                case (UInt16)PropertyType.PT_OBJECT:
                    return new MvVarSizeValue(propertyTag, propValueLength);
                default:
                    if ((propertyTag.PropType & 0x8000) == 0x8000)
                    {
                        return new MvVarSizeValue(propertyTag, propValueLength);
                    }
                    else
                        throw new ArgumentException(string.Format("The type {0} is not the Mv type.", propertyTag.PropType.ToString("X4")));
            }
        }

        internal MvVarSizeItem CreateMvVarSizeItem(PropertyTag propertyTag)
        {
            return new MvVarSizeItem(propertyTag);
        }
    }
}
