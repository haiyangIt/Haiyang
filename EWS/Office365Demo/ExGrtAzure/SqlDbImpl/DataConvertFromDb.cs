using EwsDataInterface;
using SqlDbImpl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDbImpl
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
