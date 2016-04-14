using EwsFrame.Manager.IF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Data
{
    public class ProgressArgs : EventArgs
    {
        public IProgressInfo ProgressInfo { get; private set; }
        public ProgressArgs(IProgressInfo progressInfo)
        {
            ProgressInfo = progressInfo;
        }
    }
}
