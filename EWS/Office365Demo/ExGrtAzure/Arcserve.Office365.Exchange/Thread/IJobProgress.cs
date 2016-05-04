using System;
using System.Collections.Generic;
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
}
