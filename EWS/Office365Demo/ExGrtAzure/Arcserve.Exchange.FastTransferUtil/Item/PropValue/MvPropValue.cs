using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.CompoundFile;

namespace Arcserve.Exchange.FastTransferUtil.Item.PropValue
{
    public class MvPropValue : FTNodeBase, IPropValue
    {
        public MvPropValue(PropertyTag propertyTag)
            : base()
        {
            PropTag = propertyTag;
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

        public IPropTag PropTag
        {
            get; set;
        }

        public IValue PropValue
        {
            get
            {
                return _SizeValue;
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
