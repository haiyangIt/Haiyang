using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsFrame;
using Arcserve.Office365.Exchange.DataProtect.Interface;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.StorageAccess.Interface;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.Data.Event;
using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.ArcJob;

namespace Arcserve.Office365.Exchange.DataProtect.Impl
{
    public class RestoreService : RestoreServiceBase, IRestoreService
    {
        public RestoreService(string adminUserName, string adminPassword, string domainName, string organizationName) : 
            base(adminUserName, adminPassword, domainName, organizationName)
        {

        }

        public RestoreService(string adminUserName, string organizationName): base(adminUserName, organizationName) { }

        private RestoreServiceBase _restoreServiceObj;
        private RestoreServiceBase RestoreServiceObj
        {
            get
            {
                if(_restoreServiceObj == null)
                {
                    _restoreServiceObj = new RestoreServiceBase(this);
                }
                return _restoreServiceObj;
            }
        }

        protected override void RestoreStart()
        {
            StartTime = DateTime.Now;
            ServiceContext.DataAccessObj.BeginTransaction();
        }

        protected override void RestoreEnd(bool isFinished)
        {
            ServiceContext.DataAccessObj.EndTransaction(isFinished);
        }

        public override void RestoreItem(string mailbox, IRestoreItemInformation item)
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreItem(mailbox, item);
                isFinish = true;
            }
            finally
            {
                RestoreEnd(isFinish);
            }
        }

        public override void RestoreOrganization()
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreOrganization();
                isFinish = true;
            }
            finally
            {
                RestoreEnd(isFinish);
            }
        }

        public override void RestoreMailboxes(List<string> mailboxes)
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreMailboxes(mailboxes);
                isFinish = true;
            }
            finally
            {
                RestoreEnd(isFinish);
            }
        }

        public override void RestoreMailbox(string mailbox)
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreMailbox(mailbox);
                isFinish = true;
            }
            finally
            {
                RestoreEnd(isFinish);
            }
        }

        public override void RestoreFolders(string mailbox, List<string> folderIds, bool isRecursion = false)
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreFolders(mailbox, folderIds, isRecursion);
                isFinish = true;
            }
            finally
            {
                RestoreEnd(isFinish);
            }
        }

        public override void RestoreFolder(string mailbox, string folderId, bool isRecursion = false)
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreFolder(mailbox, folderId, isRecursion);
                isFinish = true;
            }
            finally
            {
                RestoreEnd(isFinish);
            }
        }

        public override void RestoreItems(string mailbox, List<IRestoreItemInformation> items)
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreItems(mailbox, items);
                isFinish = true;
            }
            finally
            {
                RestoreEnd(isFinish);
            }
        }

        public override void RestoreItem(string mailbox, string itemId, string displayName)
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreItem(mailbox, itemId, displayName);
                isFinish = true;
            }
            finally
            {
                RestoreEnd(isFinish);
            }
        }

    }

    public class RestoreServiceBase : IRestoreService
    {
        protected RestoreServiceBase(string adminUserName, string adminPassword, string domainName, string organizationName)
        {
            if (string.IsNullOrEmpty(adminUserName))
                throw new ArgumentException("user name is null", "adminUserName");
            if (string.IsNullOrEmpty(adminPassword))
                throw new ArgumentException("password is null", "adminPassword");

            AdminInfo = new OrganizationAdminInfo();
            AdminInfo.UserName = adminUserName;
            AdminInfo.UserPassword = adminPassword;
            AdminInfo.UserDomain = domainName;
            AdminInfo.OrganizationName = organizationName;

            ServiceContext = Arcserve.Office365.Exchange.DataProtect.Impl.Context.ServiceContext.NewServiceContext(AdminInfo.UserName, AdminInfo.UserPassword, AdminInfo.UserDomain, AdminInfo.OrganizationName, TaskType.Restore);
        }

        protected RestoreServiceBase(string adminUserName, string organizationName) : this(adminUserName, string.Empty, string.Empty, organizationName)
        {

        }

        internal RestoreServiceBase(RestoreServiceBase restoreService)
        {
            AdminInfo = restoreService.AdminInfo;

            ServiceContext = restoreService.ServiceContext;

            _restoreJobName = restoreService.RestoreJobName;
            Destination = restoreService.Destination;
            StartTime = restoreService.StartTime;
            CurrentRestoreCatalogJob = restoreService.CurrentRestoreCatalogJob;
        }

        private string _restoreJobName;
        public virtual string RestoreJobName
        {
            get
            {
                if (string.IsNullOrEmpty(_restoreJobName))
                {
                    _restoreJobName = string.Format("{0} Restore Job {1}", AdminInfo.OrganizationName, StartTime.ToString("yyyyMMddHHmmss"));
                }
                return _restoreJobName;
            }
            set
            {
                _restoreJobName = value;
            }
        }
        
        public virtual OrganizationAdminInfo AdminInfo
        {
            get; set;
        }

        public virtual ICatalogJob CurrentRestoreCatalogJob { get; set; }

        public virtual IRestoreDestination Destination
        {
            get; set;
        }

        public virtual IServiceContext ServiceContext { get; protected set; }

        public virtual DateTime StartTime { get; protected set; }

        protected IQueryCatalogDataAccess DataAccess
        {
            get
            {
                var result = (IQueryCatalogDataAccess)ServiceContext.DataAccessObj;
                result.CatalogJob = CurrentRestoreCatalogJob;
                return result;
            }
        }

        protected IDataConvertFromDb DataConvert
        {
            get
            {
                return RestoreFactory.Instance.NewDataConvert();
            }
        }

        public virtual void RestoreFolder(string mailbox, string folderId, bool isRecursion = false)
        {
            List<string> childFolders;
            if (isRecursion)
            {
                childFolders = GetAllFolderIds(mailbox, folderId);
            }
            else
            {
                childFolders = new List<string>();
                childFolders.Add(folderId);
            }
            
            foreach(var folder in childFolders)
            {
                RestoreEachFolderItems(mailbox, folder);
            }
        }

        private void RestoreEachFolderItems(string mailbox, string folderId)
        {
            List<IItemData> childItems = DataAccess.GetAllChildItems(folderId);
            List<IRestoreItemInformation> itemInformations = new List<IRestoreItemInformation>(childItems.Count);
            foreach (var item in childItems)
            {
                var itemInformation = GetRestoreItemInformation(mailbox, folderId, item.ItemId, item.DisplayName, item.ItemClass);
                RestoreItem(mailbox, itemInformation);
            }
        }

        public virtual void RestoreFolders(string mailbox, List<string> folderIds, bool isRecursion = false)
        {
            List<string> childFolders;
            if (isRecursion)
            {
                childFolders = GetAllFolderIds(mailbox, folderIds);
            }
            else
            {
                childFolders = folderIds;
            }

            foreach (var folder in childFolders)
            {
                RestoreEachFolderItems(mailbox, folder);
            }
        }

        public virtual void RestoreItem(string mailbox, string itemId, string displayName)
        {
            IItemData itemDetails = DataAccess.GetItemContent(itemId, displayName, Destination.ExportType);
            IItemData item = DataAccess.GetItem(itemId);
            var itemInformation = GetRestoreItemInformation(mailbox, item.ParentFolderId, item.ItemId, item.DisplayName, item.ItemClass);
            Destination.WriteItem(itemInformation, itemDetails.Data as byte[]);
        }

        public virtual void RestoreItem(string mailbox, IRestoreItemInformation item)
        {
            IItemData itemDetails = DataAccess.GetItemContent(item.ItemId, item.DisplayName, Destination.ExportType);
            Destination.WriteItem(item, itemDetails.Data as byte[]);
        }

        public virtual void RestoreItems(string mailbox, List<IRestoreItemInformation> items)
        {
            foreach (var item in items)
            {
                RestoreItem(mailbox, item);
            }
        }

        public virtual void RestoreMailbox(string mailbox)
        {
            var folders = DataAccess.GetAllFoldersInMailboxes(mailbox);
            var folderIds = (from s in folders select s.FolderId).ToList();
            RestoreFolders(mailbox, folderIds, false);
        }

        public virtual void RestoreMailboxes(List<string> mailboxes)
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreOrganization()
        {
            IOrganizationData data = DataConvert.Convert(AdminInfo.OrganizationName);
            var mailboxes = DataAccess.GetAllMailbox();
            var mailboxAddresses = (from s in mailboxes select s.MailAddress).ToList();
            RestoreMailboxes(mailboxAddresses);
        }

        protected virtual void RestoreStart()
        {

        }

        protected virtual void RestoreEnd(bool isFinished)
        {

        }

        protected virtual void OnProgressChanged()
        {

        }

        protected virtual void OnMailboxProgressChanged()
        {

        }

        protected virtual void OnFolderProgressChanged()
        {

        }

        protected virtual void OnItemProcessChanged()
        {

        }

        private MailboxFolderCache _FolderCache = new MailboxFolderCache();

        public event EventHandler<RestoreProgressArgs> ProgressChanged;
        public event EventHandler<RestoreMailboxArgs> MailboxeProgressChanged;
        public event EventHandler<RestoreFolderArgs> FolderProgressChanged;
        public event EventHandler<RestoreItemArgs> ItemProgressChanged;

        private void InitMailBoxFolderPathes(string mailbox)
        {
            if (!_FolderCache.IsMailboxInit(mailbox))
            {
                var allFolders = DataAccess.GetAllFoldersInMailboxes(mailbox);
                _FolderCache.AddMailboxFolders(mailbox, allFolders);
            }
        }

        private IRestoreItemInformation GetRestoreItemInformation(string mailAddress, string folderId, string itemId, string displayName, string itemClass)
        {
            InitMailBoxFolderPathes(mailAddress);
            List<IFolderDataBase> result = null;
            if (!_FolderCache.TryGetFolderPath(mailAddress, folderId, out result))
            {
                throw new ArgumentException(string.Format("can find [{0}]'s folder [{1}] pathes", mailAddress, folderId));
            }
            return new RestoreItemInformationImpl()
            {
                FolderPathes = result,
                ItemId = itemId,
                DisplayName = displayName,
                ItemClass = ItemClassUtil.GetItemClass(itemClass)
            };
        }

        private List<string> GetAllFolderIds(string mailbox, string folderId)
        {
            InitMailBoxFolderPathes(mailbox);
            return _FolderCache.GetFolderIds(mailbox, folderId);
        }

        private List<string> GetAllFolderIds(string mailbox, List<string> folderIds)
        {
            InitMailBoxFolderPathes(mailbox);
            return _FolderCache.GetFolderIds(mailbox, folderIds);
        }

        class MailboxFolderCache
        {
            private Dictionary<string, MailboxFolderTreePath> Mailbox2FolderPath = new Dictionary<string, MailboxFolderTreePath>();

            public bool IsMailboxInit(string mailbox)
            {
                return Mailbox2FolderPath.ContainsKey(mailbox);
            }

            public bool TryGetFolderPath(string mailbox, string folderId, out List<IFolderDataBase> paths)
            {
                MailboxFolderTreePath result = null;
                if (Mailbox2FolderPath.TryGetValue(mailbox, out result))
                {
                    paths = result.FolderPaths[folderId];
                    return true;
                }
                else
                {
                    paths = null;
                    return false;
                }
            }

            public void AddMailboxFolders(string mailbox, List<IFolderData> folders)
            {
                MailboxFolderTreePath result = null;
                if (!Mailbox2FolderPath.TryGetValue(mailbox, out result))
                {
                    TreeNode treeroot = TreeNode.CreateTree(folders);
                    Dictionary<string, List<IFolderDataBase>> eachFolderPath = TreeNode.GetEachFolderPath(treeroot);
                    result = new MailboxFolderTreePath(mailbox, eachFolderPath, treeroot);
                    Mailbox2FolderPath.Add(mailbox, result);
                }
            }

            public List<string> GetFolderIds(string mailbox, string folderId)
            {
                MailboxFolderTreePath result = null;
                if (Mailbox2FolderPath.TryGetValue(mailbox, out result))
                {
                    return TreeNode.GetAllFoldersAndChildFolders(result.Root, folderId);
                }
                else
                    return null;
            }

            public List<string> GetFolderIds(string mailbox, List<string> folderIds)
            {
                MailboxFolderTreePath result = null;
                if (Mailbox2FolderPath.TryGetValue(mailbox, out result))
                {
                    return TreeNode.GetAllFoldersAndChildFolders(result.Root, folderIds);
                }
                else
                    return null;
            }

            private class MailboxFolderTreePath
            {
                internal readonly string MailboxAddress;
                internal readonly Dictionary<string, List<IFolderDataBase>> FolderPaths;
                internal readonly TreeNode Root;
                internal MailboxFolderTreePath(string mailboxAddress, Dictionary<string, List<IFolderDataBase>> folderPaths, TreeNode root)
                {
                    MailboxAddress = mailboxAddress;
                    FolderPaths = folderPaths;
                    Root = root;
                }

            }
        }

        class RestoreItemInformationImpl : IRestoreItemInformation
        {
            public string ItemId
            {
                get; set;
            }
            

            public List<IFolderDataBase> FolderPathes { get; set; }

            public string DisplayName
            {
                get; set;
            }

            public ItemClass ItemClass
            {
                get; set;
            }
        }
    }
}
