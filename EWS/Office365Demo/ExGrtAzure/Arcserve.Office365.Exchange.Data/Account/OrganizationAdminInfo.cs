using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Account
{
    public class OrganizationAdminInfo
    {
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserDomain { get; set; }
        public string OrganizationName { get; set; }
    }

    public static class OrganizationExtension
    {
        public static string GetOrganization(this string mailAddress)
        {
            return "Office365";
            //var address = mailAddress.Replace(".com", "");
            //address = address.Replace(".cn", "");
            //address = address.Replace(".net", "");

            //int dotPlace = address.IndexOf("@");
            //if (dotPlace < 0)
            //    throw new ArgumentException("Invalid mail addresss.");


            //address = address.Substring(dotPlace + 1, address.Length - dotPlace - 1);
            //return address.Replace(".", "_");
        }
    }
}
