using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Util
{
    public class PerformanceCounter
    {
        DateTime now;
        public PerformanceCounter()
        {

        }
        
        public void Restart()
        {
            now = DateTime.Now;
        }
        public static PerformanceCounter Start()
        {
            var result = new PerformanceCounter();
            result.Restart();
            return result;
        }

        public double EndBySecond()
        {
            return (DateTime.Now - now).TotalSeconds;
        }

        public double EndByHour()
        {
            return (DateTime.Now - now).TotalHours;
        }

        public double EndByMinute()
        {
            return (DateTime.Now - now).TotalMinutes;
        }
    }
}
