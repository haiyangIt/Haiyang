using Arcserve.Office365.Exchange.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Thread
{
    public interface IJobProgress : IProgress<double>
    {
        void Report(double val, string message);
        void Report(string message);

        void Report(string format, params object[] args);
    }

    public class JobProgress : IJobProgress
    {
        public void Report(double value)
        {
            Debug.WriteLine("Progress : {0}", value);
         
        }

        public void Report(string message)
        {
            //Debug.WriteLine(string.Format("Progress : {0}", message));
            LogFactory.LogInstance.WriteLog(LogLevel.INFO, message);
        }

        public void Report(string format, params object[] args)
        {
            //Debug.WriteLine(string.Format("Progress : {0}", string.Format(format, args)));
            LogFactory.LogInstance.WriteLog(LogLevel.INFO, "", format, args);
        }

        public void Report(double val, string message)
        {
            //Debug.WriteLine("Progress : {0}, message:{1}.", val, message);
            LogFactory.LogInstance.WriteLog(LogLevel.INFO, message);
        }
    }
}
