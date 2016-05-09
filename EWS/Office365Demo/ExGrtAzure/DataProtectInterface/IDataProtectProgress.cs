using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DataProtectInterface
{
    public interface IDataProtectProgress
    {
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        string CurrentMailbox { get;  }
        string CurrentFolder { get;  }
        string CurrentItem { get;  }
        string MailboxPercent { get;  }
        string FolderPercent { get; }
        string ItemPercent { get; }
        ConcurrentQueue<string> LatestInformation { get;  }
        string CurrentStatus { get;  }
    }
}
