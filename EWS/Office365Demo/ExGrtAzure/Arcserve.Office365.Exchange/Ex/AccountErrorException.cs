using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Ex
{
    public class AccountErrorException : Exception
    {
        public AccountErrorException() : base() { }

        public AccountErrorException(Exception inner):base("user name or password invalid.", inner) { }

        public AccountErrorException(string message, Exception inner) : base(message, inner) { }
    }

    public class AccountImpersonateException : AccountErrorException
    {
        public AccountImpersonateException() : base() { }

        public AccountImpersonateException(Exception inner):base("The account does not have permission to impersonate the requested user.", inner) { }

        public AccountImpersonateException(string message, Exception inner) : base(message, inner) { }
    }
}
