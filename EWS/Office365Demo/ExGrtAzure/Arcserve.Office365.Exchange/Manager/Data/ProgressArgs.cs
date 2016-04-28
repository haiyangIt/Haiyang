using Arcserve.Office365.Exchange.Manager.IF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Manager.Data
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
