using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastTransferUtil.CompoundFile;

namespace FTStreamUtil.Item.PropValue
{
    public class FixPropValue : FTNodeBase, IPropValue
    {
        public FixPropValue(PropertyTag propertyTag)
            : base()
        {
            PropTag = propertyTag;
            _fixedPropType = FTFactory.Instance.CreateFixedPropType();
            _propInfo = FTFactory.Instance.CreatePropInfo(propertyTag);
            _fixedSizeValue = FTFactory.Instance.CreateFixedSizeValue(_fixedPropType, propertyTag.PropType);

            Children.Add(_fixedPropType);
            Children.Add(_propInfo);
            Children.Add(_fixedSizeValue);
        }

        private FixedPropType _fixedPropType;
        private IPropInfo _propInfo;
        private IFixedSizeValue _fixedSizeValue;
        
        public IPropTag PropTag
        {
            get; set;
        }

        public IValue PropValue
        {
            get
            {
                return _fixedSizeValue;
            }
        }

        public IPropInfo PropInfo
        {
            get
            {
                return _propInfo;
            }
        }

        public override void WriteToCompoundFile(CompoundFileBuild build)
        {
            build.AddProperty(this);
        }
    }
}
