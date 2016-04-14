using EwsFrame.Manager.IF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
{
    public class ProgressInfoBase : IProgressInfo
    {
        public IArcJob Job
        {
            get; set;
        }

        public string Organization
        {
            get; set;
        }

        public DateTime Time
        {
            get; set;
        }
    }
}
