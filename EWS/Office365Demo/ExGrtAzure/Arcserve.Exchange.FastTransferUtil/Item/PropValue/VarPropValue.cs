using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.CompoundFile;

namespace Arcserve.Exchange.FastTransferUtil.Item.PropValue
{
    public class VarPropValue : FTNodeBase, IPropValue
    {
        public VarPropValue(PropertyTag propertyTag)
            : base()
        {
            PropTag = propertyTag;
            _varPropType = FTFactory.Instance.CreateVarPropType();
            _propInfo = FTFactory.Instance.CreatePropInfo(propertyTag);
            _length = FTFactory.Instance.CreatePropValueLength();
            _varSizeValue = FTFactory.Instance.CreateVarSizeValue(_varPropType, _length, propertyTag.PropType);

            Children.Add(_varPropType);
            Children.Add(_propInfo);
            Children.Add(_length);
            Children.Add(_varSizeValue);
        }

        private VarPropType _varPropType;
        private IPropInfo _propInfo;
        private PropValueLength _length;
        private IVarSizeValue _varSizeValue;

        public IPropTag PropTag
        {
            get; set;
        }

        public IValue PropValue
        {
            get
            {
                return _varSizeValue;
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
