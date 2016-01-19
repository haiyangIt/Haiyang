using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.PropValue
{
    public interface IPropValue : IFTTreeNode, IFTTransferUnit
    {
        IPropTag PropTag { get; set; }

        IValue PropValue { get; }

        IPropInfo PropInfo { get; }
    }
    
    public interface IPropTag
    {
        uint PropertyTag { get; }
        ushort PropertyId { get; }
        ushort PropertyType { get; }
        byte[] Bytes { get; }
    }

    public interface IValue : IFTTreeNode
    {
        byte[] BytesForMsg { get; }

        int BytesCountForMsg { get; }
    }
}
