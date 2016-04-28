using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface
{
    public class CatalogException : ApplicationException
    {
        public CatalogException(string message, Exception innerException):base(message, innerException)
        { }
    }
}
