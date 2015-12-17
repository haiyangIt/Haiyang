using Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public JsonResult UpdateMailboxes(DateTime catalogDateTime)
        {
            UpdateMailboxesModel result = new UpdateMailboxesModel();
            result.CatalogTime = catalogDateTime;

            var tree = new Tree();
            var allMailboxes = tree.GetAllMailbox();
            List<Item> infos = new List<Item>(allMailboxes.Count);
            foreach (var mailbox in allMailboxes)
            {
                infos.Add(new Item() { Id = mailbox.RootFolderId, DisplayName = mailbox.DisplayName, ChildCount = tree.GetChildCount(mailbox.RootFolderId), ItemType = "Mailbox", OtherInformation = mailbox});
            }

            result.Details = infos;
            return Json(result);
        }

        public JsonResult UpdateFolders(DateTime catalogDateTime, string rootFolderId)
        {
            var tree = new Tree();
            List<Item> result = GetContainer(rootFolderId, tree);

            UpdateMailboxesModel jsonModel = new UpdateMailboxesModel();
            jsonModel.CatalogTime = catalogDateTime;
            jsonModel.Details = result;

            return Json(jsonModel);
        }

        private List<Item> GetContainer(string folderId, Tree tree)
        {
            var allRootFolder = tree.GetDirectFolder(folderId);
            List<Item> result = new List<Item>();
            foreach (var folder in allRootFolder)
            {
                var item = new Item() { Id = folder.FolderId, ChildCount = tree.GetChildCount(folder.FolderId), DisplayName = folder.DisplayName, ItemType = "Folder" };
                item.Container = GetContainer(folder.FolderId, tree);
                result.Add(item);
            }
            return result;
        }

        //public JsonResult UpdateFolders(DateTime catalogDateTime, string rootFolderId)
        //{
        //    UpdateMailboxesModel result = new UpdateMailboxesModel();
        //    result.CatalogTime = catalogDateTime;
        //    List<Item> infos = new List<Item>(24);

        //    string i = "0";
        //    var mailboxInfo0 = new FolderInfo()
        //    {
        //        DisplayName = string.Format("Folder-DisplayName-{0}", i),
        //        MailboxAddress = string.Format("Address@arcserve.com", i),
        //        FolderId = i.ToString(),
        //        ChildFolderCount = 2,
        //        ChildItemCount = 4
        //    };
        //    var mailboxInfo = mailboxInfo0;
        //    var item0 = new Item() { Id = mailboxInfo.FolderId, DisplayName = mailboxInfo.DisplayName, Count = mailboxInfo.ChildItemCount, OtherInformation = mailboxInfo };

        //    i = "01";
        //    var mailboxInfo01 = new FolderInfo()
        //    {
        //        DisplayName = string.Format("Folder-DisplayName-{0}", i),
        //        MailboxAddress = string.Format("Address@arcserve.com", i),
        //        FolderId = i.ToString(),
        //        ChildFolderCount = 0,
        //        ChildItemCount = 2,
        //        ParentFolderId = "0"
        //    };
        //    mailboxInfo = mailboxInfo01;
        //    var item01 = new Item() { Id = mailboxInfo.FolderId, DisplayName = mailboxInfo.DisplayName, Count = mailboxInfo.ChildItemCount, OtherInformation = mailboxInfo };

        //    i = "02";
        //    var mailboxInfo02 = new FolderInfo()
        //    {
        //        DisplayName = string.Format("Folder-DisplayName-{0}", i),
        //        MailboxAddress = string.Format("Address@arcserve.com", i),
        //        FolderId = i.ToString(),
        //        ChildFolderCount = 0,
        //        ChildItemCount = 2,
        //        ParentFolderId = "0"
        //    };
        //    mailboxInfo = mailboxInfo02;
        //    var item02 = new Item() { Id = mailboxInfo.FolderId, DisplayName = mailboxInfo.DisplayName, Count = mailboxInfo.ChildItemCount, OtherInformation = mailboxInfo };
        //    item0.Children = new List<Item>() { item01, item02 };

        //    i = "1";
        //    var mailboxInfo1 = new FolderInfo()
        //    {
        //        DisplayName = string.Format("Folder-DisplayName-{0}", i),
        //        MailboxAddress = string.Format("Address@arcserve.com", i),
        //        FolderId = i.ToString(),
        //        ChildFolderCount = 0,
        //        ChildItemCount = 1
        //    };
        //    mailboxInfo = mailboxInfo1;
        //    var item1 = new Item() { Id = mailboxInfo.FolderId, DisplayName = mailboxInfo.DisplayName, Count = mailboxInfo.ChildItemCount, OtherInformation = mailboxInfo };

        //    i = "2";
        //    var mailboxInfo2 = new FolderInfo()
        //    {
        //        DisplayName = string.Format("Folder-DisplayName-{0}", i),
        //        MailboxAddress = string.Format("Address@arcserve.com", i),
        //        FolderId = i.ToString(),
        //        ChildFolderCount = 1,
        //        ChildItemCount = 2,
        //    };
        //    mailboxInfo = mailboxInfo2;
        //    var item2 = new Item() { Id = mailboxInfo.FolderId, DisplayName = mailboxInfo.DisplayName, Count = mailboxInfo.ChildItemCount, OtherInformation = mailboxInfo };

        //    i = "21";
        //    var mailboxInfo21 = new FolderInfo()
        //    {
        //        DisplayName = string.Format("Folder-DisplayName-{0}", i),
        //        MailboxAddress = string.Format("Address@arcserve.com", i),
        //        FolderId = i.ToString(),
        //        ChildFolderCount = 2,
        //        ChildItemCount = 2,
        //        ParentFolderId = "2"
        //    };
        //    mailboxInfo = mailboxInfo21;
        //    var item21 = new Item() { Id = mailboxInfo.FolderId, DisplayName = mailboxInfo.DisplayName, Count = mailboxInfo.ChildItemCount, OtherInformation = mailboxInfo };
        //    item2.Children = new List<Item>() { item21 };


        //    i = "211";
        //    var mailboxInfo211 = new FolderInfo()
        //    {
        //        DisplayName = string.Format("Folder-DisplayName-{0}", i),
        //        MailboxAddress = string.Format("Address@arcserve.com", i),
        //        FolderId = i.ToString(),
        //        ChildFolderCount = 0,
        //        ChildItemCount = 1000,
        //        ParentFolderId = "21"
        //    };
        //    mailboxInfo = mailboxInfo211;
        //    var item211 = new Item() { Id = mailboxInfo.FolderId, DisplayName = mailboxInfo.DisplayName, Count = mailboxInfo.ChildItemCount, OtherInformation = mailboxInfo };


        //    i = "212";
        //    var mailboxInfo212 = new FolderInfo()
        //    {
        //        DisplayName = string.Format("Folder-DisplayName-{0}", i),
        //        MailboxAddress = string.Format("Address@arcserve.com", i),
        //        FolderId = i.ToString(),
        //        ChildFolderCount = 0,
        //        ChildItemCount = 2,
        //        ParentFolderId = "21"
        //    };
        //    mailboxInfo = mailboxInfo212;
        //    var item212 = new Item() { Id = mailboxInfo.FolderId, DisplayName = mailboxInfo.DisplayName, Count = mailboxInfo.ChildItemCount, OtherInformation = mailboxInfo };
        //    item21.Children = new List<Item>() { item211, item212 };


        //    infos.Add(item0);
        //    infos.Add(item1);
        //    infos.Add(item2);

        //    result.Details = infos;
        //    return Json(result);
        //}

        public JsonResult UpdateMails(string folderId, int pageIndex, int pageCount)
        {
            var tree = new Tree();
            var mails = tree.GetMails(folderId);
            int startIndex = pageIndex * pageCount;
            int endIndex = startIndex + pageCount;
            List<Item> result = new List<Item>(pageCount);
            while (startIndex < endIndex && startIndex < mails.Count)
            {
                var mailItem = mails[startIndex];
                var item = new Item() { Id = mailItem.ItemId, ChildCount = 0, DisplayName = mailItem.DisplayName, OtherInformation = mailItem };
                
                result.Add(item);

                startIndex++;
            }
            return Json(new MailResult() { Mails = result, TotalCount = mails.Count });
        }

        //public JsonResult UpdateMails(string folderId, int pageIndex, int pageCount)
        //{
        //    int startIndex = pageIndex * pageCount;
        //    int endIndex = startIndex + pageCount;
        //    List<MailInfo> result = new List<MailInfo>(pageCount);
        //    while (startIndex < endIndex && startIndex < 45)
        //    {
        //        MailInfo item = new MailInfo()
        //        {
        //            ItemId = startIndex.ToString(),
        //            Size = startIndex * 10 * 1024,
        //            DisplayName = string.Format("Mail-DisplayName-{0}", startIndex),
        //            CreateTime = DateTime.Now + new TimeSpan(0, startIndex, 0),
        //            ParentFolderId = folderId
        //        };
        //        result.Add(item);
        //        startIndex++;
        //    }
        //    return Json(new MailResult() { Mails = result, TotalCount = 45 });
        //}
    }

    public class Tree
    {
        public List<MailboxInfo> GetAllMailbox()
        {
            return _allMails;
        }

        public List<FolderInfo> GetAllFolders(string mailboxRootFolderId)
        {
            List<FolderInfo> result;
            if (_mailbox2Folders.TryGetValue(mailboxRootFolderId, out result))
            {
                return result;
            }
            return new List<FolderInfo>(0);

        }

        public int GetChildCount(string id)
        {
            int result;
            if (_folder2TotalChildCount.TryGetValue(id, out result))
            {
                return result;
            }
            return 0;
        }

        public List<FolderInfo> GetDirectFolder(string id)
        {
            List<FolderInfo> result;
            if(_folder2Children.TryGetValue(id, out result)){
                return result;
            }
            return new List<FolderInfo>(0);
        }
       
        public List<MailInfo> GetMails(string folderId)
        {
            List<MailInfo> result;
            if(_folder2Mails.TryGetValue(folderId, out result))
            {
                return result;
            }
            return new List<MailInfo>(0);
        }

        public List<MailboxInfo> _allMails;
        public Dictionary<string, List<FolderInfo>> _mailbox2Folders;
        public Dictionary<string, List<MailInfo>> _folder2Mails;
        public Dictionary<string, int> _folder2TotalChildCount;
        public Dictionary<string, List<FolderInfo>> _folder2Children;

        public Tree()
        {
            _allMails = new List<MailboxInfo>(2);
            _allMails.Add(new MailboxInfo() { DisplayName = "user1", MailAddress = "mailbox1@arcserve.com", RootFolderId = "Mailbox-1" });
            _allMails.Add(new MailboxInfo() { DisplayName = "user2", MailAddress = "mailbox2@arcserve.com", RootFolderId = "Mailbox-2" });
            
           
            var folder1 = new FolderInfo() { DisplayName = "Inbox", FolderId = "Folder1-Inbox", ParentFolderId = "Mailbox-1", ChildFolderCount = 2, ChildItemCount = 12 };

            #region folder1 child items
            var item11 = new MailInfo() { DisplayName = "item11", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item11", Size = 100 };
            var item12 = new MailInfo() { DisplayName = "item12", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item12", Size = 101 };
            var item13 = new MailInfo() { DisplayName = "item13", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item13", Size = 1002 };
            var item14 = new MailInfo() { DisplayName = "item14", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item14", Size = 1003 };
            var item15 = new MailInfo() { DisplayName = "item15", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item15", Size = 1004 };
            var item16 = new MailInfo() { DisplayName = "item16", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item16", Size = 1005 };
            var item17 = new MailInfo() { DisplayName = "item17", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item17", Size = 1006 };
            var item18 = new MailInfo() { DisplayName = "item18", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item18", Size = 1007 };
            var item19 = new MailInfo() { DisplayName = "item19", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item19", Size = 1008 };
            var item110 = new MailInfo() { DisplayName = "item110", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item110", Size = 1009 };
            var item111 = new MailInfo() { DisplayName = "item111", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item111", Size = 10010 };
            var item112 = new MailInfo() { DisplayName = "item112", ParentFolderId = "Folder1-Inbox", CreateTime = DateTime.Now, ItemId = "folder1-item112", Size = 10011 };
            #endregion

            var folder11 = new FolderInfo() { DisplayName = "Inbox-Child1", FolderId = "Folder-Inbox-Child1", ParentFolderId = "Folder1-Inbox", ChildFolderCount = 0, ChildItemCount = 1 };
            #region folder11 child items
            var item11_1 = new MailInfo() { DisplayName = "item11_1", ParentFolderId = "Folder-Inbox-Child1", CreateTime = DateTime.Now, ItemId = "folder1-item11_1", Size = 100 };
            #endregion

            var folder12 = new FolderInfo() { DisplayName = "Inbox-Child2", FolderId = "Folder-Inbox-Child2", ParentFolderId = "Folder1-Inbox", ChildFolderCount = 1, ChildItemCount = 2 };
            #region folder11 child items
            var item12_1 = new MailInfo() { DisplayName = "item12_1", ParentFolderId = "Folder-Inbox-Child2", CreateTime = DateTime.Now, ItemId = "folder1-item12_1", Size = 102 };
            var item12_2 = new MailInfo() { DisplayName = "item12_2", ParentFolderId = "Folder-Inbox-Child2", CreateTime = DateTime.Now, ItemId = "folder1-item12_2", Size = 109 };
            #endregion

            var folder121 = new FolderInfo() { DisplayName = "Inbox-Child2-Child1", FolderId = "Folder-Inbox-Child2-Child1", ParentFolderId = "Folder-Inbox-Child2", ChildFolderCount = 0, ChildItemCount = 1 };
            #region folder11 child items
            var item121_1 = new MailInfo() { DisplayName = "item121_1", ParentFolderId = "Folder-Inbox-Child2-Child1", CreateTime = DateTime.Now, ItemId = "folder1-item121_1", Size = 102 };
            #endregion

            var folder2 = new FolderInfo() { DisplayName = "Inbox", FolderId = "Folder2-Inbox", ParentFolderId = "Mailbox-2", ChildFolderCount = 0, ChildItemCount = 2 };
            #region folder11 child items
            var item2_1 = new MailInfo() { DisplayName = "item2_1", ParentFolderId = "Folder2-Inbox", CreateTime = DateTime.Now, ItemId = "folder2-item2_1", Size = 102 };
            var item2_2 = new MailInfo() { DisplayName = "item2_2", ParentFolderId = "Folder2-Inbox", CreateTime = DateTime.Now, ItemId = "folder2-item2_2", Size = 109 };
            #endregion

            _folder2Children = new Dictionary<string, List<FolderInfo>>();
            List<FolderInfo> tempChildFolders = new List<FolderInfo>();
            tempChildFolders.Add(folder1);
            _folder2Children.Add("Mailbox-1", tempChildFolders);

            tempChildFolders = new List<FolderInfo>();
            tempChildFolders.Add(folder11);
            tempChildFolders.Add(folder12);
            _folder2Children.Add(folder1.FolderId, tempChildFolders);

            tempChildFolders = new List<FolderInfo>();
            tempChildFolders.Add(folder121);
            _folder2Children.Add(folder12.FolderId, tempChildFolders);


            tempChildFolders = new List<FolderInfo>();
            tempChildFolders.Add(folder2);
            _folder2Children.Add("Mailbox-2", tempChildFolders);

            _mailbox2Folders = new Dictionary<string, List<FolderInfo>>();
            List<FolderInfo> allFolders = new List<FolderInfo>();
            allFolders.Add(folder1);
            allFolders.Add(folder11);
            allFolders.Add(folder12);
            allFolders.Add(folder121);
            
            _mailbox2Folders.Add("Mailbox-1", allFolders);

            allFolders = new List<FolderInfo>();

            allFolders.Add(folder2);
            _mailbox2Folders.Add("Mailbox-2", allFolders);

            _folder2Mails = new Dictionary<string, List<MailInfo>>();
            var listFolder1 = new List<MailInfo>();
            listFolder1.Add(item11);
            listFolder1.Add(item12);
            listFolder1.Add(item13);
            listFolder1.Add(item14);
            listFolder1.Add(item15);
            listFolder1.Add(item16);
            listFolder1.Add(item17);
            listFolder1.Add(item18);
            listFolder1.Add(item19);
            listFolder1.Add(item110);
            listFolder1.Add(item111);
            listFolder1.Add(item112);
            _folder2Mails.Add(folder1.FolderId, listFolder1);

            listFolder1 = new List<MailInfo>();
            listFolder1.Add(item11_1);
            _folder2Mails.Add(folder11.FolderId, listFolder1);

            listFolder1 = new List<MailInfo>();
            listFolder1.Add(item12_1);
            listFolder1.Add(item12_2);
            _folder2Mails.Add(folder12.FolderId, listFolder1);

            listFolder1 = new List<MailInfo>();
            listFolder1.Add(item121_1);
            _folder2Mails.Add(folder121.FolderId, listFolder1);

            listFolder1 = new List<MailInfo>();
            listFolder1.Add(item2_1);
            listFolder1.Add(item2_2);
            _folder2Mails.Add(folder2.FolderId, listFolder1);

            _folder2TotalChildCount = new Dictionary<string, int>();
            _folder2TotalChildCount.Add("Mailbox-1", 1);
            _folder2TotalChildCount.Add(folder1.FolderId, folder1.ChildFolderCount + folder1.ChildItemCount);
            _folder2TotalChildCount.Add(folder11.FolderId, folder11.ChildItemCount + folder11.ChildFolderCount);
            _folder2TotalChildCount.Add(folder12.FolderId, folder12.ChildItemCount + folder12.ChildFolderCount);
            _folder2TotalChildCount.Add(folder121.FolderId, folder121.ChildItemCount + folder121.ChildFolderCount);
            
            _folder2TotalChildCount.Add("Mailbox-2", 2);
            _folder2TotalChildCount.Add(folder2.FolderId, folder2.ChildItemCount + folder2.ChildFolderCount);
        }
    }
}