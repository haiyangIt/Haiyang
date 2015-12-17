using EwsDataInterface;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TableBlobImpl.Access.Table;

namespace TableBlobImpl.Storage.Table.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// The tablename is organization name + "mail".
    /// The rowkey is mailAddress.
    /// The partitionkey is ticks.
    /// </remarks>
    public class MailboxEntity : TableEntity, IMailboxData, ICatalogInfo
    {
        public MailboxEntity() { }

        [IgnoreProperty()]
        public string MailAddress { get; set; }

        [IgnoreProperty()]
        public string OrganizationName { get; set; }

        public string DisplayName
        {
            get; set;
        }

        [IgnoreProperty()]
        public string Location
        {
            get
            {
                return OrganizationName;
            }
            set
            {
                OrganizationName = value;
            }
        }

        [IgnoreProperty()]
        public string RootFolderId
        {
            get; set;
        }

        [IgnoreProperty()]
        public DateTime StartTime
        {
            get; set;
        }

        public int ChildFolderCount
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ItemKind ItemKind
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static MailboxEntity CreateMailboxEntityFromEws(string organizationName, string displayName, string mailboxAddress, DateTime catalogStartTime)
        {
            MailboxEntity entity = new MailboxEntity();
            entity.DisplayName = displayName;
            entity.MailAddress = mailboxAddress;
            entity.OrganizationName = organizationName;
            entity.StartTime = catalogStartTime;
            entity.RowKey = TableDataAccess.ValidateRowPartitionKey(mailboxAddress, false);
            long ticks = DateTime.MaxValue.Ticks - catalogStartTime.Ticks;
            entity.PartitionKey = TableDataAccess.ValidateRowPartitionKey(ticks.ToString(), false);

            return entity;
        }

        public static MailboxEntity CreateMailboxEntityFromDb(string organizationName, string mailboxName)
        {
            throw new NotImplementedException();
        }

        internal static string GetMailTableName(string organization, DateTime startTime)
        {
            return string.Format("{0}{1}", organization.ToLower(), "mail");
        }

        public IMailboxData Clone()
        {
            throw new NotImplementedException();
        }
    }
}