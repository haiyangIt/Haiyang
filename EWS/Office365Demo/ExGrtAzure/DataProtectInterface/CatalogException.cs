using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public class CatalogException : ApplicationException
    {
        public CatalogException(string message, Exception innerException):base(message, innerException)
        { }
    }
}
