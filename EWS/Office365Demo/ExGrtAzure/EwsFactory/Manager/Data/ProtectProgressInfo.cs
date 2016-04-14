using DataProtectInterface.Event;
using EwsFrame.Manager.IF;
using EwsFrame.Manager.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Data
{
    public class ProtectProgressInfo : ProgressInfoBase
    {
    }

    public class BackupProgressInfo : ProtectProgressInfo
    {
        public CatalogProgressArgs ProgressInfo { get; set; }
        
        public BackupProgressInfo() : base()
        {
            
        }

        public BackupProgressInfo(CatalogProgressArgs progressInfo, IArcJob jobInfo) : this()
        {
            ProgressInfo = progressInfo;
            Job = jobInfo;
            Time = progressInfo.Time;
        }
    }
}
