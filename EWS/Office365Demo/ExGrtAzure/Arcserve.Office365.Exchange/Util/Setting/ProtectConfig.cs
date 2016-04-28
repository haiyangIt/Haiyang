using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Increment
{
    public class ProtectConfig
    {
        public int ParallelItemMaxNumber { get; }
        public int ParallelFolderMaxNumber { get; }
        public int ParallelMailboxMaxNumber { get; }
        public int OnceLoadpropertiesForItemsCount { get; }
        public PropertySet PropertiesForItem { get; }
    }
}
