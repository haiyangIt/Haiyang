using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public interface IPropInfo : IFTTreeNode
    {
    }

    public class TagPropId : PropertyId , IPropInfo
    {
        
    }

    public class NamePropInfo : FTNodeBase,IPropInfo
    {
        public NamePropInfo()
            : base()
        {
            _namedPropId = FTFactory.Instance.CreateNamedPropId();
            _propertySet = FTFactory.Instance.CreatePropertySet();
            _x00Or01 = FTFactory.Instance.CreateX00Or01();
            _dispIdOrName = FTFactory.Instance.CreateDispIdOrNameBase(_x00Or01);

            Children.Add(_namedPropId);
            Children.Add(_propertySet);
            Children.Add(_x00Or01);
            Children.Add(_dispIdOrName);
        }

        private NamedPropId _namedPropId;
        private PropertySet _propertySet;
        private X00Or01 _x00Or01;
        private IDispIdOrName _dispIdOrName;
    }

    public class NamedPropId : PropertyId
    {

    }

    public class PropertyId : FTNodeLeaf<UInt16>
    {
        protected override ushort ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadUInt16();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X4");
        }

        public override int WriteLeafData(IFTStreamWriter writer)
        {
            int count = writer.Write(Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return FTStreamConst.UInt16Size;
            }
        }
    }
    

    public interface IDispIdOrName : IFTTreeNode
    { }

    public class DispIdOrNameBase: FTOneNode<IDispIdOrName>, IDispIdOrName
    {
        private X00Or01 _x00or01;
        public DispIdOrNameBase(X00Or01 x00or01)
            : base()
        {
            _x00or01 = x00or01;
        }

        protected override IDispIdOrName CreateItem(PropertyTag propertyTag)
        {
            return FTFactory.Instance.CreateDispIdOrName(_x00or01);
        }

        public override bool IsTagRight(PropertyTag propertyTag)
        {
            return true;
        }
    }

    public class X00Or01 : FTNodeLeaf<byte>
    {
        public const byte X00ForDispId = 0x00;
        public const byte X01ForName = 0x01;

        protected override byte ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadByte();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X2");
        }



        public override int WriteLeafData(IFTStreamWriter writer)
        {
            int count = writer.Write(Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return FTStreamConst.ByteSize;
            }
        }
    }

    public class PropertySet : FTNodeLeaf<Guid>
    {
        protected override Guid ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadGuid();
        }

        public override string GetLeafString()
        {
            return Data.ToString();
        }

        public override int WriteLeafData(IFTStreamWriter writer)
        {
            int count = writer.Write(Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return FTStreamConst.GuidSize;
            }
        }
    }

    public class DispId : FTNodeLeaf<UInt32>, IDispIdOrName
    {

        protected override uint ReadLeafData(IFTStreamReader reader)
        {
            return reader.ReadUInt32();
        }

        public override string GetLeafString()
        {
            return Data.ToString("X8");
        }

        public override int WriteLeafData(IFTStreamWriter writer)
        {
            int count = writer.Write(Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return FTStreamConst.UInt32Size;
            }
        }
    }

    public class Name : FTNodeLeaf<string>, IDispIdOrName
    {
        protected override string ReadLeafData(IFTStreamReader reader)
        {
            bool isReadTerminate = false;
            return reader.ReadUnicodeString(out isReadTerminate);
        }

        public override string GetLeafString()
        {
            return Data;
        }

        public override int WriteLeafData(IFTStreamWriter writer)
        {
            int count = writer.WriteUnicodeString(Data);
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        public override int BytesCount
        {
            get
            {
                return (Data.Length + 1)*2;
            }
        }
    }
}
