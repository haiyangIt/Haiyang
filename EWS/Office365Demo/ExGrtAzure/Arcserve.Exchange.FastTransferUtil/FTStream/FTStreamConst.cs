using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.FTStream
{
    public class FTStreamConst
    {
        internal const int UInt16Size = 2;
        internal const int UInt32Size = 4;
        internal const int UInt64Size = 8;
        internal const int FloatSize = 4;
        internal const int DoubleSize = 8;
        internal const int GuidSize = 16;
        internal const int DateTimeSize = 8;
        internal const int CurrencySize = 8;
        internal const int ByteSize = 1;
        internal const int BooleanSize = 2;

        /// <summary>
        /// Boolen size in fast transfer is 2
        /// </summary>
        internal const int BoolenSizeInFT = 2;
        internal const ushort BoolenTrueInFT = 0x0001;
        internal const ushort BoolenFalseInFT = 0x0000;
    }
}
