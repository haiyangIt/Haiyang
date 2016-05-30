using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.FTStream
{
    public enum FxOpcodes
    {
        None = 0,
        Config = 1,
        TransferBuffer = 2,
        IsInterfaceOk = 3,
        TellPartnerVersion = 4,
        StartMdbEventsImport = 11,
        FinishMdbEventsImport = 12,
        AddMdbEvents = 13,
        SetWatermarks = 14,
        SetReceiveFolder = 15,
        SetPerUser = 0x10,
        SetProps = 0x11
    }
}
