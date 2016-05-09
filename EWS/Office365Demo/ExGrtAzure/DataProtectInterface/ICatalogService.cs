using DataProtectInterface.Event;
using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface ICatalogService : ICatalogServiceEvent , IDisposable
    {
        DateTime StartTime { get; }

        //DateTime LastCatalogTime { get; }

        string CatalogJobName { get; set; }

        OrganizationAdminInfo AdminInfo { get; }

        IServiceContext ServiceContext { get; }

        List<IMailboxData> GetAllUserMailbox();

        List<IFolderData> GetFolder(string mailbox, string parentId, bool containRootFolder);

        void GenerateCatalog();

        void GenerateCatalog(IFilterItem filter);
    }

    
}
