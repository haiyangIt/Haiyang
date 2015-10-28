using DataProtectInterface.Event;
using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface ICatalogService : ICatalogServiceEvent
    {
        DateTime StartTime { get; }

        DateTime LastCatalogTime { get; }

        string CatalogJobName { get; }

        OrganizationAdminInfo AdminInfo { get; }

        IServiceContext ServiceContext { get; }

        List<IMailboxData> GetAllUserMailbox();

        void GenerateCatalog();

        void GenerateCatalog(string mailbox);
        void GenerateCatalog(string mailbox, string folder);
    }

    
}
