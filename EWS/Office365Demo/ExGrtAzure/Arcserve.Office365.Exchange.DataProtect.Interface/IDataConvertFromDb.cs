using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface
{
    public interface IDataConvertFromDb
    {
        IOrganizationData Convert(string organizationName);
        IItemData Convert(IItemData itemData, byte[] data);
    }
}
