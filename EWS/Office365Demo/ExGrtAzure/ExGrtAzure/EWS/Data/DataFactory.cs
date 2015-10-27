using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExGrtAzure.EWS.Data
{
    public class DataFactory
    {
        public static DataFactory Instance
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IDataConvert NewDataConvert()
        {
            return new DataConvert();
        }

        public IDataAccess NewDataAccess()
        {
            return new TableBlobDataAccess();
        }
    }
}