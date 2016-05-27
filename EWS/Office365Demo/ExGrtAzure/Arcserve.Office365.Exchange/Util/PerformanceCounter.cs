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

        private TimeSpan GetTotalTimeSpan()
        {
            TimeSpan result = new TimeSpan(0);
            foreach (var t in EachSuspendTime)
            {
                result += t;
            }
            return result;
        }

        public double EndBySecond()
        {
            EachSuspendTime.Add(DateTime.Now - now);
            return GetTotalTimeSpan().TotalSeconds;
        }

        public double EndByHour()
        {
            EachSuspendTime.Add(DateTime.Now - now);
            return GetTotalTimeSpan().TotalHours;
        }

        public double EndByMinute()
        {
            EachSuspendTime.Add(DateTime.Now - now);
            return GetTotalTimeSpan().TotalMinutes;
        }

        public void Suspend()
        {
            var end = DateTime.Now;
            EachSuspendTime.Add(end - now);
        }
        public void Suspend(bool isRecord)
        {
            var end = DateTime.Now;
            if (isRecord)
                EachSuspendTime.Add(end - now);
        }

        public void Reset()
        {
            now = DateTime.Now;
            EachSuspendTime.Clear();
        }
        public void Resume()
        {
            now = DateTime.Now;
        }

        public void DoForEachTimeSpan(Action<TimeSpan> action)
        {
            foreach (var t in EachSuspendTime)
            {
                action(t);
            }
        }

        public List<TimeSpan> EachSuspendTime = new List<TimeSpan>();
    }
}
