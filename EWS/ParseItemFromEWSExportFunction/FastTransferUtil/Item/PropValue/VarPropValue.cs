using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastTransferUtil.CompoundFile;

namespace FTStreamUtil.Item.PropValue
{
    public class VarPropValue : FTNodeBase, IPropValue
    {
        public VarPropValue(PropertyTag propertyTag)
            : base()
        {
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


        public override void WriteToCompoundFile(CompoundFileBuild build)
        {
            build.AddVarPropValue(this);
        }

    }
}
