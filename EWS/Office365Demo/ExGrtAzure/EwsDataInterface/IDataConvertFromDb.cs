using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IDataConvertFromDb
    {
        IOrganizationData Convert(string organizationName);
        IItemData Convert(IItemData itemData, byte[] data);
    }
}
