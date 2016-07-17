using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Arcserve.Office365.Exchange.Com
{
    [Guid("B0825EC9-46E0-4D83-BA0B-7B848A4B6787")]
    [ComVisible(true)]
    public interface IQueryCondition
    {
        string SortField { get; set; }
        ContentFilter ContentFilter { get; set; }
        string SearchString { get; set; }
    }

    

    [ComVisible(true)]
    public enum ContentFilter
    {
        CONTENT_MAIL,
        CONTENT_FOLDER,
        CONTENT_CALENDAR,
        CONTENT_CONTACT,
        CONTENT_ALL
    }
}
