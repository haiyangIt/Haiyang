using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Mail
{
    public enum ItemClass
    {
        None = 0,
        Message = 1,
        Appointment = 2,
        Contact = 3,
        Task = 4
    }

    public enum FolderClass
    {
        None = 0,
        Message = 1,
        Calendar = 2,
        Contact = 3,
        Task = 4
    }
}
