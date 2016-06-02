using Arcserve.Office365.Exchange.Data.Increment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi.Interface
{
    public interface IExportItemsOper
    {
        void WriteBufferToStorage(IItemDataSync itemId, byte[] buffer, int length);
        void WriteComplete(IItemDataSync itemId);
        void ExportItemError(EwsResponseException ewsResponseError);
    }
}
