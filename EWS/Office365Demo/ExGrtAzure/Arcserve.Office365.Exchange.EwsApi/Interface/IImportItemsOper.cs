using Arcserve.Office365.Exchange.Data.Increment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi.Interface
{
    public interface IImportItemsOper
    {
        int ReadDataFromStorage(IItemDataSync item, byte[] buffer, int offset, int length);
        void ImportItemError(EwsResponseException ewsResponseError);
    }
}
