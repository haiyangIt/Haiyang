using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public interface ISizeValue : IFTTreeNode
    {
    }

    public class MvFixedSizeValue : FTNodeCollection<IFixedSizeValue>, ISizeValue
    {
        private PropertyTag _tag;
        private PropValueLength _length;
        private int _parsed = 0;
        public MvFixedSizeValue(PropertyTag propertyTag, PropValueLength length)
        {
            _tag = propertyTag;
            _length = length;
        }

        protected override IFixedSizeValue CreateItem(PropertyTag propertyTag)
        {
            FixedPropType propType = FTFactory.Instance.CreateFixedPropType();
            ushort propTypeInt = (ushort)(_tag.PropType & 0xEFFF);
            propType.Init(propTypeInt);
            return FTFactory.Instance.CreateFixedSizeValue(propType, propTypeInt);
        }

        public override bool IsTagRight(PropertyTag propertyTag)
        {
            if (_parsed++ < _length.Data)
                return true;
            return false;
        }
    }

    public class MvVarSizeValue : FTNodeCollection<MvVarSizeItem>, ISizeValue
    {
        private PropertyTag _tag;
        private PropValueLength _length;
        private int _parsed = 0;
        public MvVarSizeValue(PropertyTag propertyTag, PropValueLength length)
            : base()
        {
            _tag = propertyTag;
            _length = length;
        
        }

        protected override MvVarSizeItem CreateItem(PropertyTag propertyTag)
        {
            return FTFactory.Instance.CreateMvVarSizeItem(_tag);
        }

        public override bool IsTagRight(PropertyTag propertyTag)
        {
            if (_parsed++ < _length.Data)
                return true;
            return false;
        }
    }

    public class MvVarSizeItem : FTNodeBase
    {
        private PropValueLength _length;
        private IVarSizeValue _varSizeValue;

        public MvVarSizeItem(PropertyTag mvPropertyTag)
            : base()
        {
            VarPropType varPropType = FTFactory.Instance.CreateVarPropType();
            ushort varPropTypeInt = (UInt16)(mvPropertyTag.PropType & 0xEFFF);
            varPropType.Init(varPropTypeInt);
            _length = FTFactory.Instance.CreatePropValueLength();
            _varSizeValue = FTFactory.Instance.CreateVarSizeValue(varPropType, _length, varPropTypeInt);

            Children.Add(_length);
            Children.Add(_varSizeValue);
        }
    }
}
