using EwsDataInterface;
using Microsoft.Exchange.WebServices.Data;
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
    /// Its tablename is organization name + "folder"
    /// Its row key is folder id.
    /// Its partition key is start time ticks.
    /// </remarks>
    public class FolderEntity :TableEntity , IFolderData, ICatalogInfo
    {
        public FolderEntity() { }
        [IgnoreProperty()]
        public string MailboxAddress { get; set; }
        [IgnoreProperty()]
        public string FolderId { get; set; }
        public string ParentFolderId { get; set; }

        [IgnoreProperty()]
        public string FolderName { get; set; }
        public string FolderType { get; set; }

        public int ChildItemCount { get; set; }
        
        #region Implement IFolderData

        [IgnoreProperty()]
        public string Location
        {
            get; set;
        }

        public string DisplayName
        {
            get
            {
                return FolderName;
            }
            set
            {
                FolderName = value;
            }
        }

        [IgnoreProperty()]
        public DateTime StartTime
        {
            get; set;
        }

        public int ChildFolderCount
        {
            get; set;
        }

        #endregion

        public static FolderEntity CreateFolderEntityFromEws(Folder folder, DateTime catalogStartTime)
        {
            FolderEntity result = new FolderEntity();
            result.FolderName = folder.DisplayName;
            result.FolderId = folder.Id.UniqueId;
            if (folder.ParentFolderId != null)
                result.ParentFolderId = folder.ParentFolderId.UniqueId;
            result.MailboxAddress = folder.Id.Mailbox.Address;
            result.FolderType = folder.FolderClass;
            result.ChildItemCount = folder.TotalCount;
            result.ChildFolderCount = folder.ChildFolderCount;
            result.StartTime = catalogStartTime;
            result.RowKey = TableDataAccess.ValidateRowPartitionKey(result.FolderId, false);
            long ticks = DateTime.MaxValue.Ticks - catalogStartTime.Ticks;
            result.PartitionKey = TableDataAccess.ValidateRowPartitionKey(ticks.ToString(), false);
            return result;
        }

        public static string GetFolderTableName(string organizationName, string mailAddress, DateTime startTime)
        {
            return string.Format("{0}{1}", organizationName.ToLower(), mailAddress);
        }

        public IFolderData Clone()
        {
            throw new NotImplementedException();
        }
    }
}