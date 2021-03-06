﻿using FTStreamUtil.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public interface IContentProcess
    {
        bool IsTagRight(PropertyTag propertyTag);
    }

    public interface ITerminateNode { }

    public interface IFTTree
    {
        IList<IFTTreeNode> Children { get; }
    }
}
