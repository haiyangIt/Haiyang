using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.IF
{
    public interface IProgressInfo
    {
        DateTime Time { get; }
        string Organization { get; }
        IArcJob Job { get; }
    }

    
}
