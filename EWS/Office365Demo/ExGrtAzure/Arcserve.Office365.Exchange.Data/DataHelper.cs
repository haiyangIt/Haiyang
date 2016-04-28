using Arcserve.Office365.Exchange.Azure;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Arcserve.Office365.Exchange.Data
{
    public class DataHelper
    {
        public static string GetOrganizationPrefix(string mailbox)
        {
            int atIndex = mailbox.IndexOf("@");
            string domain = mailbox.Substring(atIndex + 1, mailbox.Length - atIndex - 1).Replace(".", "-").ToLower();
            return domain;
        }

        public static string GetItemContainerName(string mailboxName, string folderIdMd5Str, int index)
        {
            return string.Format("{0}{3}{1}{3}{2}", GetOrganizationPrefix(mailboxName), folderIdMd5Str, index, BlobDataAccess.DashChar);
        }

        public static string GetItemContainerName(IItemData itemData, string mailboxName)
        {
            return GetItemContainerName(mailboxName, MD5Utility.ConvertToMd5(itemData.ParentFolderId), 0);
        }

        public static string GetItemNextContainerName(string containerName, string mailboxName)
        {
            string[] array = containerName.Split(BlobDataAccess.DashCharArray, StringSplitOptions.RemoveEmptyEntries);
            int index = Convert.ToInt32(array[1]);
            return GetItemContainerName(mailboxName, array[0], index + 1);
        }

        public static string GetLocation(IItemData item, string mailboxName)
        {
            return GetItemContainerName(item, mailboxName);
        }

        public static string GetFolderContainerMappingBlobName(string name)
        {
            return HttpUtility.UrlEncode(name);
        }
    }
}
