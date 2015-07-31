using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class StreamUtil
    {
        delegate IPropType CreateFixedPropType(UInt16 type);
        delegate IPropValue ParsePropInfo(IPropType propType, byte[] buffer, ref int pos);
        delegate IFixedSizeValue CreateFixedPropValue();
        delegate IVarSizeValue CreateVarPropValue();

        private static Dictionary<UInt16, CreateFixedPropType> CreatePropTypeDic = new Dictionary<ushort, CreateFixedPropType>();
        private static Dictionary<UInt16, ParsePropInfo> ParsePropInfoDic = new Dictionary<ushort, ParsePropInfo>();
        private static Dictionary<UInt16, CreateFixedPropValue> CreatePropValueDic = new Dictionary<ushort, CreateFixedPropValue>();
        private static Dictionary<UInt16, CreateVarPropValue> CreateVarPropValueDic = new Dictionary<ushort, CreateVarPropValue>();
        public static void Initialize()
        {

            CreatePropTypeDic.Add((UInt16)TagType.PT_I2, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_I8, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_LONG, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_R4, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_DOUBLE, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_CURRENCY, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_APPTIME, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_BOOLEAN, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_SYSTIME, PtypInteger16.CreateFixedPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_CLSID, PtypInteger16.CreateFixedPropType);

            CreatePropTypeDic.Add((UInt16)TagType.PT_STRING8, PtypInteger16.CreateVarPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_UNICODE, PtypInteger16.CreateVarPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_SERVERID, PtypInteger16.CreateVarPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_OBJECT, PtypInteger16.CreateVarPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_BINARY, PtypInteger16.CreateVarPropType);

            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_I2, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_LONG, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_R4, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_DOUBLE, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_CURRENCY, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_APPTIME, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_SYSTIME, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_STRING8, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_BINARY, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_UNICODE, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_CLSID, PtypInteger16.CreateMvPropType);
            CreatePropTypeDic.Add((UInt16)TagType.PT_MV_I8, PtypInteger16.CreateMvPropType);


            ParsePropInfoDic.Add((UInt16)TagType.PT_I2, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_I8, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_LONG, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_R4, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_DOUBLE, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_CURRENCY, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_APPTIME, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_BOOLEAN, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_SYSTIME, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_CLSID, FixPropType_PropInfo_FixedSizeValue.ParseFixPropType_PropInfo_FixedSizeValue);

            ParsePropInfoDic.Add((UInt16)TagType.PT_STRING8, VarPropType_PropInfo_Length_VarSizeValue.ParseVarPropType_PropInfo_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_UNICODE, VarPropType_PropInfo_Length_VarSizeValue.ParseVarPropType_PropInfo_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_SERVERID, VarPropType_PropInfo_Length_VarSizeValue.ParseVarPropType_PropInfo_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_OBJECT, VarPropType_PropInfo_Length_VarSizeValue.ParseVarPropType_PropInfo_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_BINARY, VarPropType_PropInfo_Length_VarSizeValue.ParseVarPropType_PropInfo_Length_VarSizeValue);


            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_I2, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_LONG, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_R4, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_DOUBLE, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_CURRENCY, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_APPTIME, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_SYSTIME, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_STRING8, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_BINARY, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_UNICODE, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_CLSID, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);
            ParsePropInfoDic.Add((UInt16)TagType.PT_MV_I8, MvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue.ParseMvPropType_PropInfo_Length_IFixSizeValue_Length_VarSizeValue);


            CreatePropValueDic.Add((UInt16)TagType.PT_I2, PtypInteger16.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_I8, PtypInteger64.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_LONG, PTypInteger32.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_R4, PTypFloating32.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_DOUBLE, PtypFloating64.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_CURRENCY, PtypCurrency.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_APPTIME, PtypFloatingTime.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_BOOLEAN, PtypBoolean.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_SYSTIME, PtypTime.CreateFixedPropValue);
            CreatePropValueDic.Add((UInt16)TagType.PT_CLSID, PTypGuid.CreateFixedPropValue);

            CreateVarPropValueDic.Add((UInt16)TagType.PT_OBJECT, PTypObject.CreateVarPropValue);
            CreateVarPropValueDic.Add((UInt16)TagType.PT_UNICODE, PTypStr.CreateVarPropValue);
            CreateVarPropValueDic.Add((UInt16)TagType.PT_STRING8, PtypString8.CreateVarPropValue);
            CreateVarPropValueDic.Add((UInt16)TagType.PT_BINARY, PTypBinary.CreateVarPropValue);
            CreateVarPropValueDic.Add((UInt16)TagType.PT_SERVERID, PTypServerId.CreateVarPropValue);

        }

        public static IPropValue ParsePropValue(byte[] buffer, ref int pos)
        {
            if (CreatePropTypeDic.Count == 0)
                Initialize();

            UInt16 type = PtypInteger16.GetPropType(buffer, ref pos);
            ParsePropInfo parsePropInfoFunc = null;
            IPropValue propValue;
            if (ParsePropInfoDic.TryGetValue(type, out parsePropInfoFunc))
            {
                IPropType propType = CreatePropTypeDic[type](type);
                propValue = parsePropInfoFunc(propType, buffer, ref pos);
            }

            else if ((type & 0x8000) == 0x8000) // It's string.
            {
                IPropType propType = PtypInteger16.CreateVarPropType(type);
                propValue = VarPropType_PropInfo_Length_VarSizeValue.ParseVarPropType_PropInfo_Length_VarSizeValue(propType, buffer, ref pos);
            }
            else
            {
                throw new NotSupportedException(string.Format("this type [{0}] doesn't support.", type.ToString("X4")));
            }

            return propValue;
        }

        public static IPropInfo CreatePropInfo(byte[] buffer, ref int pos)
        {
            UInt16 propId = PtypInteger16.GetPropId(buffer, ref pos);

            if (propId < 0x8000)
            {
                ITaggedPropId tagPropId = PtypInteger16.CreateTagPropId(propId);
                return tagPropId;
            }
            else
            {
                INamedPropId namedPropId = PtypInteger16.CreateNamedPropId(propId);
                IPropInfo namedPropInfo = NamedPropId_NamedPropInfo.CreateNamedPropId_NamedPropInfo(namedPropId, buffer, ref pos);
                return namedPropInfo;
            }
        }

        public static IFixedSizeValue CreateFixedSizeValue(IFixedPropType fixedPropType, byte[] buffer, ref int pos)
        {
            var int16Value = (fixedPropType as PtypInteger16).Value;
            CreateFixedPropValue createFunc = null;
            if (CreatePropValueDic.TryGetValue(int16Value, out createFunc))
            {
                IFixedSizeValue fixValueObj = createFunc();
                fixValueObj.ParseValue(buffer, ref pos);
                return fixValueObj;
            }
            throw new ArgumentException(string.Format("fixedPropType [{0}] is invalid.", (fixedPropType as PtypInteger16).Value.ToString("X4")));
        }


        internal static IX00Or01 CreateX00Or01(byte[] buffer, ref int pos)
        {
            byte value = PTypByte.GetX00OrX01(buffer, ref pos);
            if (value == X00.Value)
            {
                IX00Or01 result = X00.CreateX00();
                result.ParseDispIdOrName(buffer, ref pos);
                return result;
            }
            else if (value == X01.Value)
            {
                IX00Or01 result = X01.CreateX01();
                result.ParseDispIdOrName(buffer, ref pos);
                return result;
            }
            else
                throw new ArgumentException("Not support namedPropInfo if value is not X00,X01");
        }

        internal static IVarSizeValue CreateVarSizeValue(IVarPropType VarPropType, ILength Length, byte[] buffer, ref int pos)
        {
            var int16Value = (VarPropType as PtypInteger16).Value;
            CreateVarPropValue createFunc = null;
            if (CreateVarPropValueDic.TryGetValue(int16Value, out createFunc))
            {
                IVarSizeValue varValueObj = createFunc();
                varValueObj.ParseValue(buffer, ref pos, (Length as PTypInteger32).Value);
                return varValueObj;
            }
            else if ((int16Value & 0x8000) == 0x8000)
            {
                IVarSizeValue varValueObj = PTypStrWithCodePage.CreateVarPropValue(VarPropType);
                varValueObj.ParseValue(buffer, ref pos, (Length as PTypInteger32).Value);
                return varValueObj;
            }

            throw new ArgumentException(string.Format("fixedPropType [{0}] is invalid.", int16Value.ToString("X4")));
        }
    }
}
