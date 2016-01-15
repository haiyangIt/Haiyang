using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item
{
    public class PropertyTag
    {
        public static PropertyTag Empty = new PropertyTag(0x00000000);

        public PropertyTag(UInt32 propTag)
        {
            Data = propTag;
            PropId = (ushort)(propTag >> 16);
            PropType = (ushort)(propTag & 0x0000FFFF);
        }

        public uint Data { get; private set; }

        public ushort PropId { get; private set; }
        public ushort PropType { get; private set; }

        #region Judge propertytag type

        #region allMarker
        public static readonly UInt32 StartTopFld = 0x40090003;
        public static readonly UInt32 EndFolder = 0x400B0003;
        public static readonly UInt32 StartSubFld = 0x400A0003;

        public static readonly UInt32 StartMessage = 0x400C0003;
        public static readonly UInt32 EndMessage = 0x400D0003;
        public static readonly UInt32 StartFAIMsg = 0x40100003;
        public static readonly UInt32 StartEmbed = 0x40010003;
        public static readonly UInt32 EndEmbed = 0x40020003;
        public static readonly UInt32 StartRecip = 0x40030003;
        public static readonly UInt32 EndToRecip = 0x40040003;
        public static readonly UInt32 NewAttach = 0x40000003;
        public static readonly UInt32 EndAttach = 0x400E0003;

        public static readonly UInt32 IncrSyncChg = 0x40120003;
        public static readonly UInt32 IncrSyncChgPartial = 0x407D0003;
        public static readonly UInt32 IncrSyncDel = 0x40130003;
        public static readonly UInt32 IncrSyncEnd = 0x40140003;
        public static readonly UInt32 IncrSyncRead = 0x402F0003;
        public static readonly UInt32 IncrSyncStateBegin = 0x403A0003;
        public static readonly UInt32 IncrSyncStateEnd = 0x403B0003;
        public static readonly UInt32 IncrSyncProgressMode = 0x4074000B;
        public static readonly UInt32 IncrSyncProgressPerMsg = 0x4075000B;
        public static readonly UInt32 IncrSyncMessage = 0x40150003;
        public static readonly UInt32 IncrSyncGroupInfo = 0x407B0102;

        public static readonly UInt32 FXErrorInfo = 0x40180003;
        #endregion

        #region allMetaProperty
        public static readonly UInt32 MetaTagFXDelProp = 0x40160003;
        public static readonly UInt32 MetaTagEcWarning = 0x400F0003;
        public static readonly UInt32 MetaTagNewFXFolder = 0x40110102;
        public static readonly UInt32 MetaTagIncrSyncGroupId = 0x407C0003;
        public static readonly UInt32 MetaTagIncrementalSyncMessagePartial = 0x407A0003;
        public static readonly UInt32 MetaTagDnPrefix = 0x4008001E;
        #endregion

        private static HashSet<uint> _markerHash = new HashSet<uint>();
        private static HashSet<uint> _metaPropertyHash = new HashSet<uint>();
        private static HashSet<ushort> _fixedTypeHash = new HashSet<ushort>();
        private static HashSet<ushort> _varTypeHash = new HashSet<ushort>();
        private static HashSet<ushort> _mvTypeHash = new HashSet<ushort>();
        private static void Init()
        {
            #region initMarkerHash
            _markerHash.Add(StartTopFld);
            _markerHash.Add(EndFolder);
            _markerHash.Add(StartSubFld);

            _markerHash.Add(StartMessage);
            _markerHash.Add(EndMessage);
            _markerHash.Add(StartFAIMsg);
            _markerHash.Add(StartEmbed);
            _markerHash.Add(EndEmbed);
            _markerHash.Add(StartRecip);
            _markerHash.Add(EndToRecip);
            _markerHash.Add(NewAttach);
            _markerHash.Add(EndAttach);

            _markerHash.Add(IncrSyncChg);
            _markerHash.Add(IncrSyncChgPartial);
            _markerHash.Add(IncrSyncDel);
            _markerHash.Add(IncrSyncEnd);
            _markerHash.Add(IncrSyncRead);
            _markerHash.Add(IncrSyncStateBegin);
            _markerHash.Add(IncrSyncStateEnd);
            _markerHash.Add(IncrSyncProgressMode);
            _markerHash.Add(IncrSyncProgressPerMsg);
            _markerHash.Add(IncrSyncMessage);
            _markerHash.Add(IncrSyncGroupInfo);

            _markerHash.Add(FXErrorInfo);
            #endregion

            #region InitMetaProperty
            _metaPropertyHash.Add(MetaTagFXDelProp);
            _metaPropertyHash.Add(MetaTagEcWarning);
            _metaPropertyHash.Add(MetaTagNewFXFolder);
            _metaPropertyHash.Add(MetaTagIncrSyncGroupId);
            _metaPropertyHash.Add(MetaTagIncrementalSyncMessagePartial);
            _metaPropertyHash.Add(MetaTagDnPrefix);
            #endregion

            #region InitFixVarMvType

            _fixedTypeHash.Add((UInt16)PropertyType.PT_I2);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_I8);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_LONG);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_R4);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_DOUBLE);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_CURRENCY);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_APPTIME);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_BOOLEAN);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_SYSTIME);
            _fixedTypeHash.Add((UInt16)PropertyType.PT_CLSID);

            _varTypeHash.Add((UInt16)PropertyType.PT_STRING8);
            _varTypeHash.Add((UInt16)PropertyType.PT_UNICODE);
            _varTypeHash.Add((UInt16)PropertyType.PT_SERVERID);
            _varTypeHash.Add((UInt16)PropertyType.PT_OBJECT);
            _varTypeHash.Add((UInt16)PropertyType.PT_BINARY);

            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_I2);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_LONG);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_R4);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_DOUBLE);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_CURRENCY);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_APPTIME);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_SYSTIME);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_STRING8);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_BINARY);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_UNICODE);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_CLSID);
            _mvTypeHash.Add((UInt16)PropertyType.PT_MV_I8);
            #endregion
        }

        public static bool IsProperty(PropertyTag propertyTag)
        {
            return (!IsMarker(propertyTag) && !IsMetaProperty(propertyTag)) &&
                (IsFixedType(propertyTag) || IsVarType(propertyTag) || IsMultiType(propertyTag));
        }

        public static bool IsMarker(PropertyTag propertyTag)
        {
            if (_markerHash.Count == 0)
                Init();

            return _markerHash.Contains(propertyTag.Data);
        }

        public static bool IsMetaProperty(PropertyTag propertyTag)
        {
            if (_markerHash.Count == 0)
                Init();

            return _metaPropertyHash.Contains(propertyTag.Data);
        }

        public static bool IsFixedType(PropertyTag propertyTag)
        {
            if (_markerHash.Count == 0)
                Init();

            return _fixedTypeHash.Contains(propertyTag.PropType);
        }

        public static bool IsVarType(PropertyTag propertyTag)
        {
            if (_markerHash.Count == 0)
                Init();


            if (_varTypeHash.Contains(propertyTag.PropType))
                return true;
            else
            {
                if((propertyTag.PropType & 0x8000) == 0x8000 && (propertyTag.PropType & 0x1000) != 0x1000)
                    return true; 
            }
            return false;
        }

        public static bool IsMultiType(PropertyTag propertyTag)
        {
            if (_markerHash.Count == 0)
                Init();


            bool result = _mvTypeHash.Contains(propertyTag.PropType);
            if(!result)
            {
                if ((propertyTag.PropType & 0x9000) == 0x9000)
                    return true;
            }
            return result;
        }
        #endregion
    }
}
