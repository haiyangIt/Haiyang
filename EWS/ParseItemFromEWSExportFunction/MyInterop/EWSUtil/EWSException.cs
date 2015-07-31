using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EWSUtil
{
    public class EWSException : ApplicationException
    {
        public EWSException() : base() { }

        public EWSException(string msg, int hResult) : base(msg)
        {
            base.HResult = hResult;
        }

        public int HResult
        {
            get
            {
                return base.HResult;
            }
            set
            {
                base.HResult = value;
            }
        }
    }
}
