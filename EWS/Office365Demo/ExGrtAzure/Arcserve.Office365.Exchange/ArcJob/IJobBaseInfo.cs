﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.ArcJob
{
    public interface IJobBaseInfo
    {
        DateTime CreateTime { get; }
        string JobName { get; }
        string JobId { get; }
        TaskType TaskType { get; }
        string PlanName { get; }
    }
}