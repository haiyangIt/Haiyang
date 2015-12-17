using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo.Models
{
    public class UpdateMailboxesModel
    {
        public DateTime CatalogTime { get; set; }
        public List<Item> Details { get; set; }
    }

    public class MailboxInfo
    {
        public string Location { get; set; }
        public string DisplayName { get; set; }
        public string MailAddress { get; set; }
        public string RootFolderId { get; set; }
    }

    public class Item
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public object OtherInformation { get; set; }
        public List<Item> Container { get; set; }
        public List<Item> Leaf { get; set; }

        /// <summary>
        /// Container.Count + leaf.Count
        /// </summary>
        public int ChildCount { get; set; }
        public string ItemType { get; set; }
    }

    public class FolderInfo
    {
        public string ParentFolderId { get; set; }
        public string MailboxAddress { get; set; }
        public string Location { get; set; }
        public string DisplayName { get; set; }
        public string FolderId { get; set; }
        public string FolderType { get; set; }
        public int ChildItemCount { get; set; }
        public int ChildFolderCount { get; set; }
    }

    public class MailInfo
    {

        public string ParentFolderId { get; set; }
        public string DisplayName { get; set; }
        public DateTime? CreateTime { get; set; }
        public string ItemId { get; set; }
        public object Data { get; set; }
        public string ItemClass { get; set; }
        public int Size { get; set; }
        public int ActualSize { get; set; }
        public string Location { get; set; }
    }

    public class MailResult
    {
        public int TotalCount { get; set; }
        public List<Item> Mails { get; set; }
    }
}