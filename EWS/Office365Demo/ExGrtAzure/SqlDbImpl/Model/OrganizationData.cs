using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SqlDbImpl.Model
{
    public class OrganizationModel : IOrganizationData
    {
        public string Name
        {
            get; set;
        }

        public static string GetOrganizationName(string mailAddress)
        {
            int dotPlace = mailAddress.LastIndexOf(".");
            if (dotPlace < 0)
                throw new ArgumentException("Invalid mail addresss.");

            int secondDotPlace = mailAddress.LastIndexOf(".", 0, dotPlace - 1);
            if (secondDotPlace < 0)
                secondDotPlace = 0;
            return mailAddress.Substring(secondDotPlace, mailAddress.Length - secondDotPlace);
        }
    }
}