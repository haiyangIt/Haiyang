using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Tool.Result
{
    public class ExchangeBackupResult : ResultBase
    {
        public ExchangeBackupResult() : base() { Status = ResultStatus.Success; }
        public ExchangeBackupResult(string errorMsg) : base(errorMsg) { }
    }
}
