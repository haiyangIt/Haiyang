using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.FTStream
{
    public interface IFTSerialize
    {
        int GetByteData(IFTStreamWriter writer);
        void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit);
    }
}
