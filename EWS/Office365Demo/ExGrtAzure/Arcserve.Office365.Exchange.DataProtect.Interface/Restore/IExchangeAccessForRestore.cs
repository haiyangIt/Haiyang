using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Restore
{
    public interface IExchangeAccessForRestore<ProgressType>
    {
        ICatalogAccessForRestore<ProgressType> CatalogAccess { get; set; }
        IEwsServiceAdapter<ProgressType> EwsServiceAdapter { get; set; }
        IDataFromClientForRestore<ProgressType> DataFromClient { get; set; }
    }

    public static class ExchangeAccessRestoreExtension
    {
        public static void CloneExchangeAccess<ProgressType>(this IExchangeAccessForRestore<ProgressType> des, IExchangeAccessForRestore<ProgressType> sour)
        {
            des.CatalogAccess = sour.CatalogAccess;
            des.EwsServiceAdapter = sour.EwsServiceAdapter;
            des.DataFromClient = sour.DataFromClient;
        }
    }
}
