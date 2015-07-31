using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public class FixPropValue : FTNodeBase, IPropValue
    {
        public FixPropValue(PropertyTag propertyTag)
            : base()
        {
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
    }
}
