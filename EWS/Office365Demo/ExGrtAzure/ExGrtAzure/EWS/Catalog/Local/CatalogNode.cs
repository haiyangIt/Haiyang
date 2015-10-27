using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExGrtAzure.EWS.Catalog.Local
{
    public class CatalogNode
    {
        public CatalogNode Parent { get; private set; }
        public object Data { get; private set; }
    }
}