using Arcserve.Office365.Exchange.Log;
using Microsoft.Practices.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Topaz
{
    public class RetryContext
    {
        public RetryContext()
        {

        }

        public RetryContext(string operationDescription, OperationType opType)
        {
            OperationDescription = operationDescription;
            OperationType = opType;
        }
        public string OperationDescription { get; set; }
        public OperationType OperationType { get; set; }

        private Action<object, RetryingEventArgs> _retryEventHandler;
        public Action<object, RetryingEventArgs> RetryEventHandler
        {
            get
            {
                if (_retryEventHandler == null)
                {
                    return DefaultRetryEventHandler;
                }
                return _retryEventHandler;
            }
            set
            {
                _retryEventHandler = value;
            }
        }

        private void DefaultRetryEventHandler(object sender, RetryingEventArgs args)
        {
            var msg = String.Format("Retry {0} - Count:{1}, Delay:{2}",
            OperationDescription, args.CurrentRetryCount, args.Delay);
            LogFactory.LogInstance.WriteLog("", LogLevel.WARN, msg);
            LogFactory.LogInstance.WriteException("", LogLevel.ERR, msg, args.LastException, args.LastException.Message);
        }

        private Exception DefaultRetryFailureHanlder(Exception e)
        {
            LogFactory.LogInstance.WriteException("", LogLevel.ERR, string.Format("Operation {0} failed.", OperationDescription), e, e.Message);
            return e;
        }

        public Func<Exception, Exception> _retryFailureHandler;
        public Func<Exception, Exception> RetryFailureHandler
        {
            get
            {
                if (_retryFailureHandler == null)
                {
                    return DefaultRetryFailureHanlder;
                }
                return _retryFailureHandler;
            }
            set
            {
                _retryFailureHandler = value;
            }
        }
    }

    public enum OperationType
    {
        Ews,
        Others
    }
}
