using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Tool.Framework
{
    class ArgumentInToolException : Exception
    {
        public ArgumentInToolException(string message) : base(string.Format("Program error: {0}.", message)) { }

        public ArgumentInToolException(string message, Exception innerException) : base(string.Format("Program error: {0}.", message), innerException) { }
    }
}
