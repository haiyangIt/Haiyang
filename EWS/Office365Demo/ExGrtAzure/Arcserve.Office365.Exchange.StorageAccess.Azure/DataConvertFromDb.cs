using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Impl.Mail;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.DataProtect.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.StorageAccess.Azure
{
    public class DataConvertFromDb : IDataConvertFromDb
    {
        public IOrganizationData Convert(string organizationName)
        {
            return new OrganizationModel()
            {
                Name = organizationName
            };
        }

        public IItemData Convert(IItemData itemData, byte[] data)
        {
            return new ItemModel()
            {
                ItemId = itemData.ItemId,
                Data = data,
                Location = itemData.Location
            };
        }
    }
}
