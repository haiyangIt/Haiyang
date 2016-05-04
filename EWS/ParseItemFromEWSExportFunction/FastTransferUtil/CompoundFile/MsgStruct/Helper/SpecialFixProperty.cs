using FTStreamUtil;
using FTStreamUtil.FTStream;
using FTStreamUtil.Item;
using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace FastTransferUtil.CompoundFile.MsgStruct.Helper
{
    internal interface IPropValueForParser : IPropValue
    {
        void Parse(IStorage storage);
    }


    internal interface ISpecialValue : IPropValueForParser { }

    internal class SpecialPropertyUtil
    {

        private static object _lock = new object();
        private static SpecialPropertyUtil _instance = null;
        public static SpecialPropertyUtil Instance
        {
            get
            {
                if (_instance == null)
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SpecialPropertyUtil();
                        }
                    }
                return _instance;
            }
        }

        public ISpecialValue CreateNewPropValue(int propertyTag, Int64 value)
        {
            PropertyTag tag = new PropertyTag((uint)propertyTag);

            if (PropertyTag.IsFixedType(tag))
            {
                return new SpecialFixProperty(propertyTag, value);
            }
            else if (PropertyTag.IsVarType(tag))
            {
                if (tag.PropType == (ushort)PropertyType.PT_UNICODE)
                    return new SpecialVarStringProperty(propertyTag, value);
                else
                    return new SpecialVarBaseProperty(propertyTag, value);
            }
            else if (PropertyTag.IsMultiType(tag))
            {
                return new SpecialMvBaseProperty(propertyTag, value);
            }
            else
                throw new InvalidProgramException();
        }

        StringBuilder sb = new StringBuilder();
        public string GetString(ISpecialValue property)
        {
            sb.Length = 0;
            sb.Append("Property Tag:[");
            sb.Append(property.PropTag.PropertyTag.ToString("X8"));
            sb.Append("].");
            sb.Append("Property Value:[");
            foreach(var b in ((IFTTreeNode)property).Bytes)
            {
                sb.Append(b.ToString("X2"));
                sb.Append(" ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    internal class SpecialFixProperty : ISpecialValue
    {
        private Int64 _parseValueFromMsg;
        public SpecialFixProperty(int tag, Int64 parseValueFromMsg)
        {
            PropTag = new PropertyTag((uint)tag);
            Tag = (uint)tag;
            _parseValueFromMsg = parseValueFromMsg;
        }

        public SpecialFixProperty(int tag, byte[] value)
        {
            PropTag = new PropertyTag((uint)tag);
            Tag = (uint)tag;
            Bytes = value;

            PropValue = new SpecialPropertyValue() { Bytes = this.Bytes, BytesCount = this.BytesCount };
        }

        public byte[] Bytes
        {
            get; set;
        }

        public int BytesCount
        {
            get
            {
                return Bytes.Length;
            }
        }

        public IList<IFTTreeNode> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropInfo PropInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropTag PropTag
        {
            get; set;
        }

        public IValue PropValue
        {
            get; set;
        }

        public uint Tag
        {
            get; set;
        }

        public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
        {
            throw new NotImplementedException();
        }

        public int GetByteData(IFTStreamWriter writer)
        {
            throw new NotImplementedException();
        }


        public void Parse(IStorage storage)
        {
            if (PropertyTag.IsGuidType(PropTag))
            {
                Bytes = CompoundFileUtil.Instance.ReadStream(BaseStruct.GetPropertyStreamName(PropTag), storage, 16);
            }
            else
            {
                var length = PropertyTag.GetFixedValueLength((ushort)PropTag.PropertyType);
                var temp = BitConverter.GetBytes(_parseValueFromMsg);
                Bytes = new byte[length];
                Array.Copy(temp, Bytes, length);
            }
        }

        public void Parse(IFTStreamReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteToCompoundFile(CompoundFileBuild build)
        {
            throw new NotImplementedException();
        }



        class SpecialPropertyValue : IValue
        {
            public byte[] Bytes
            {
                get; set;
            }

            public int BytesCount
            {
                get; set;
            }

            public byte[] BytesForMsg
            {
                get
                {
                    return Bytes;
                }
            }

            public int BytesCountForMsg
            {
                get
                {
                    return BytesCount;
                }
            }

            public IList<IFTTreeNode> Children
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
            {
                throw new NotImplementedException();
            }

            public int GetByteData(IFTStreamWriter writer)
            {
                throw new NotImplementedException();
            }

            public void Parse(IFTStreamReader reader)
            {
                throw new NotImplementedException();
            }

            public void WriteToCompoundFile(CompoundFileBuild build)
            {
                throw new NotImplementedException();
            }
        }
    }

    internal class SpecialVarBinaryProperty : SpecialVarBaseProperty
    {
        public SpecialVarBinaryProperty(int tag, Int64 parseValueFromMsg) : base(tag, parseValueFromMsg) { }

        public SpecialVarBinaryProperty(uint tag, byte[] value) : base(tag, value)
        {

        }
    }

    internal class SpecialVarStringProperty : SpecialVarBaseProperty
    {
        public SpecialVarStringProperty(int tag, Int64 parseValueFromMsg) : base(tag, parseValueFromMsg) { }
        public SpecialVarStringProperty(uint tag, byte[] value) : base(tag, value)
        {

        }
        public override int BytesCount
        {
            get
            {
                return Bytes.Length + 2;
            }
        }

        public override void Parse(IStorage storage)
        {
            int length = (int)_parseValueFromMsg;
            Bytes = CompoundFileUtil.Instance.ReadStream(BaseStruct.GetPropertyStreamName(PropTag), storage, length - 2);
        }
    }

    internal class SpecialVarBaseProperty : ISpecialValue
    {
        protected Int64 _parseValueFromMsg;
        public SpecialVarBaseProperty(int tag, Int64 parseValueFromMsg)
        {
            PropTag = new PropertyTag((uint)tag);
            Tag = (uint)tag;
            _parseValueFromMsg = parseValueFromMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value">doesnot contain the terminate character.</param>
        public SpecialVarBaseProperty(uint tag, byte[] value)
        {
            Bytes = value;
            Tag = tag;
            PropTag = new PropertyTag(tag);
            PropValue = new SpecialVarStrValue(Bytes, BytesCount);
        }

        public virtual byte[] Bytes
        {
            get; set;
        }

        public virtual int BytesCount
        {
            get
            {
                return Bytes.Length;
            }
        }

        public IList<IFTTreeNode> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropInfo PropInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropTag PropTag
        {
            get; set;
        }

        public IValue PropValue
        {
            get; set;
        }

        public uint Tag
        {
            get; set;
        }

        public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
        {
            throw new NotImplementedException();
        }

        public int GetByteData(IFTStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        public virtual void Parse(IStorage storage)
        {
            if (PropTag.PropertyType == (short)PropertyType.PT_OBJECT)
            {
                Bytes = BitConverter.GetBytes(_parseValueFromMsg);
                //Debug.WriteLine(string.Format("Property [{0:X8}] skip.", PropTag.PropertyTag));
                return;
            }
            int length = (int)_parseValueFromMsg;
            Bytes = CompoundFileUtil.Instance.ReadStream(BaseStruct.GetPropertyStreamName(PropTag), storage, length);
        }

        public void Parse(IFTStreamReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteToCompoundFile(CompoundFileBuild build)
        {
            throw new NotImplementedException();
        }


    }

    internal class SpecialMvBaseProperty : ISpecialValue
    {
        private Int64 _parseValueFromMsg;
        public SpecialMvBaseProperty(int tag, Int64 parseValueFromMsg)
        {
            PropTag = new PropertyTag((uint)tag);
            _parseValueFromMsg = parseValueFromMsg;
        }

        public byte[] Bytes
        {
            get; set;
        }

        public int BytesCount
        {
            get
            {
                return Bytes.Length;
            }
        }

        public IList<IFTTreeNode> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropInfo PropInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPropTag PropTag
        {
            get; set;
        }

        public IValue PropValue
        {
            get; set;
        }

        public uint Tag
        {
            get
            {
                return (uint)PropTag.PropertyTag;
            }
        }

        public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
        {
            throw new NotImplementedException();
        }

        public int GetByteData(IFTStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Parse(IFTStreamReader reader)
        {
            throw new NotImplementedException();
        }

        public void Parse(IStorage storage)
        {
            if (PropertyTag.IsMultiVarType((ushort)PropTag.PropertyTag))
            {
                List<int> sizes = new List<int>();
                int totalSize = 0;

                // read length stream;
                int sizeStreamLength = (int)_parseValueFromMsg;
                byte[] sizeBytes = CompoundFileUtil.Instance.ReadStream(BaseStruct.GetPropertyStreamName(PropTag), storage, sizeStreamLength);
                int sizeTemp = 0;
                if ((ushort)PropTag.PropertyType == (ushort)PropertyType.PT_MV_BINARY)
                {
                    using (MemoryStream stream = new MemoryStream(sizeBytes))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        while (reader.BaseStream.Length > reader.BaseStream.Position)
                        {
                            sizeTemp = reader.ReadInt32();
                            totalSize += sizeTemp;
                            sizes.Add(sizeTemp);
                            reader.ReadInt32();
                        }
                    }
                }
                else
                {
                    using (MemoryStream stream = new MemoryStream(sizeBytes))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        while (reader.BaseStream.Length > reader.BaseStream.Position)
                        {
                            sizeTemp = reader.ReadInt32();
                            totalSize += sizeTemp;
                            sizes.Add(sizeTemp);
                        }
                    }
                }

                Bytes = new byte[totalSize];

                using (MemoryStream stream = new MemoryStream(Bytes))
                {
                    int index = 0;
                    foreach (int sizeItem in sizes)
                    {
                        byte[] temp = CompoundFileUtil.Instance.ReadStream(BaseStruct.GetMvVarPropertyStreamName(PropTag, index), storage, sizeItem);
                        stream.Write(temp, 0, sizeItem);
                        index++;
                    }
                }
            }
            else
            {
                int length = (int)_parseValueFromMsg;
                Bytes = CompoundFileUtil.Instance.ReadStream(BaseStruct.GetPropertyStreamName(PropTag), storage, length);
            }
        }

        public void WriteToCompoundFile(CompoundFileBuild build)
        {
            throw new NotImplementedException();
        }
    }

    class SpecialVarStrValue : IValue
    {
        public SpecialVarStrValue(byte[] bytes, int byteCount)
        {
            Bytes = bytes;
            BytesCount = byteCount;
        }
        public byte[] Bytes
        {
            get; set;
        }

        public int BytesCount
        {
            get; set;
        }

        public int BytesCountForMsg
        {
            get
            {
                return BytesCount;
            }
        }

        public byte[] BytesForMsg
        {
            get
            {
                return Bytes;
            }
        }

        public IList<IFTTreeNode> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
        {
            throw new NotImplementedException();
        }

        public int GetByteData(IFTStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Parse(IFTStreamReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteToCompoundFile(CompoundFileBuild build)
        {
            throw new NotImplementedException();
        }
    }
}
