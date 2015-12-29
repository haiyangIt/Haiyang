using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastTransferUtil.CompoundFile;

namespace FTStreamUtil.Item.PropValue
{
    public class MvPropValue : FTNodeBase, IPropValue
    {
        public MvPropValue(PropertyTag propertyTag)
            : base()
        {
            _mvPropType = FTFactory.Instance.CreateMvPropType();
            _propInfo = FTFactory.Instance.CreatePropInfo(propertyTag);
            _length = FTFactory.Instance.CreatePropValueLength();
            _SizeValue = FTFactory.Instance.CreateSizeValue(propertyTag, _length);

            Children.Add(_mvPropType);
            Children.Add(_propInfo);
            Children.Add(_length);
            Children.Add(_SizeValue);
        }

        private MvPropType _mvPropType;
        private IPropInfo _propInfo;
        private PropValueLength _length;
        private ISizeValue _SizeValue;

        public override void WriteToCompoundFile(CompoundFileBuild build)
        {
            build.AddMvPropValue(this);
        }
    }
}
