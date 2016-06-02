using Arcserve.Office365.Exchange.Data.Mail;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi.Increment
{
    public class ItemDatas
    {
        public Item Item { get; set; }
        public Stream GetItemStream(string itemId, ExportType type)
        {
            throw new NotImplementedException();
        }

        public byte[] GetItemBytes(string itemId, ExportType type)
        {
            throw new NotImplementedException();
        }
    }
    
}
