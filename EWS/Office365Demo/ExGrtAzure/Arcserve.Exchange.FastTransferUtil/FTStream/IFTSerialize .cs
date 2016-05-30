using Arcserve.Exchange.FastTransferUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.FTStream
{
    public interface IFTSerialize
    {
        int GetByteData(IFTStreamWriter writer);
        void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit);
    }
}
