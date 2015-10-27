using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExGrtAzure.EWS.DataAccess;

namespace ExGrtAzure.EWS.Catalog.Local
{
    public class CatalogToDiskService : CatalogService
    {
        public string SessionLocation { get; private set; }
        public CatalogToDiskService(string sessionLocation, string adminUserName, string adminPassword, string domainName) 
            : base(adminUserName, adminPassword, domainName)
        {
            SessionLocation = sessionLocation;
        }

        public override IDataAccess NewDataAccessInstance()
        {
            return new DataAccessDisk();
        }
    }
}