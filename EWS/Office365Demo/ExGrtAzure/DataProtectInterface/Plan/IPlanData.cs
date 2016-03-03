using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface.Plan
{
    public interface IPlanData
    {
        string Name { get; }
        string Organization { get; }
        string CredentialInfo { get; }
    }

    public class ScheduleInfo
    {
        public DateTime StartTime { get; set; }
        public int Interval { get; set; }
        public FrequencyEnum Frequency { get; set; }
        public DateTime EndTime { get; set; }
        public int RecurCount { get; set; }
        public Schedule Schedules { get; set; }
    }

    public class Schedule
    {
        public int Minutes { get; set; }
        public int Hours { get; set; }
        public int WeekDays { get; set; }
        public int MonthDays { get; set; }
        public MonthlyOccurrence MonthlyOccurrences { get; set; }
    }

    public class MonthlyOccurrence
    {
        public int WeekDay { get; set; }
        public int WeekDayInMonthPlace { get; set; }
    }

    public enum FrequencyEnum
    {
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year
    }

}
