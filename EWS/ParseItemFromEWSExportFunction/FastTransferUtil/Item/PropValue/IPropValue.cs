using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public interface IPropValue : IFTTreeNode, IFTTransferUnit
    {
        int PropertyTag { get; }
        int PropertyType { get; }
        int PropertyId { get; }

        IPropInfo PropInfo { get; }
    }
}
