using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface.Event
{
    public interface ICatalogServiceEvent
    {
        event EventHandler<CatalogProgressArgs> ProgressChanged;
        //event EventHandler<CatalogMailboxArgs> MailboxProgressChanged;
        //event EventHandler<CatalogFolderArgs> FolderProgressChanged;
        //event EventHandler<CatalogItemArgs> ItemProgressChanged;
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
