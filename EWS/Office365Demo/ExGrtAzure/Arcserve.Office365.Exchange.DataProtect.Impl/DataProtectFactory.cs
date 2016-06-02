using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.DataProtect.Impl.Backup;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Impl
{
    public class DataProtectFactory
    {
        static DataProtectFactory()
        {
            Instance = new DataProtectFactory();
        }
        public static DataProtectFactory Instance;

        public BackupFlowTemplate NewBackupInstance(IExchangeAccess<IJobProgress> dataExchange, OrganizationAdminInfo adminUserInfo)
        {
            var result = new SyncBackup();
            result.CloneExchangeAccess(dataExchange);
            result.AdminInfo = adminUserInfo;
            return result;
        }

        public BackupFlowTemplate NewBackupInstance(ICatalogAccess<IJobProgress> CatalogAccess,
                                    IEwsServiceAdapter<IJobProgress> EwsServiceAdapter,
                                    IDataFromClient<IJobProgress> DataFromClient,
                                    IDataConvert DataConvert, OrganizationAdminInfo adminUserInfo)
        {
            var result = new SyncBackup();
            result.CatalogAccess = CatalogAccess;
            result.EwsServiceAdapter = EwsServiceAdapter;
            result.DataConvert = DataConvert;
            result.DataFromClient = DataFromClient;
            result.AdminInfo = adminUserInfo;
            return result;
        }

        public ITaskSyncContext<IJobProgress> NewDefaultTaskSyncContext()
        {
            return new TaskSyncContextBase();
        }
    }
}
