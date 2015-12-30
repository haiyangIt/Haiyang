using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public interface IPropValue : IFTTreeNode, IFTTransferUnit
    {        
        IPropType PropType { get; }
        IPropTag PropTag { get; }

        IValue PropValue { get; }

        IPropInfo PropInfo { get; }
    }


    public interface IPropType : IFTTreeNode
    {
        Int16 PropertyType { get; }
    }

    public interface IPropTag : IFTTreeNode
    {
        Int32 PropertyTag { get; }
        Int16 PropertyId { get; }
        Int16 PropertyType { get; set; }
    }

    public interface IValue: IFTTreeNode
    {

    }
}
