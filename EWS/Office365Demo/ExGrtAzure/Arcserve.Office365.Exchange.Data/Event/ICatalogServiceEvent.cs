using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Event
{
    public interface ICatalogServiceEvent
    {
        event EventHandler<CatalogProgressArgs> ProgressChanged;
        event EventHandler<EventExceptionArgs> ExceptionThrowed;
    }
    
    public class Process 
    {
        int CurrentIndex;
        int TotalCount;

        public Process() { }
        public Process(int currentIndex, int totalCount)
        {
            CurrentIndex = currentIndex;
            TotalCount = totalCount;
        }
    }
}
