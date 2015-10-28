using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using EwsFrame;
using DataProtectInterface.Event;

namespace DataProtectImpl
{
    public class RestoreService : RestoreServiceBase, IRestoreService
    {
        public RestoreService(string adminUserName, string adminPassword, string domainName, string organizationName) : 
            base(adminUserName, adminPassword, domainName, organizationName)
        {

        }

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

        public void RestoreStart()
        {
            StartTime = DateTime.Now;
            RestoreFactory.Instance.NewDataAccess().BeginTransaction();
        }

        public void RestoreEnd(bool isFinished)
        {
            RestoreFactory.Instance.NewDataAccess().EndTransaction(isFinished);
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

        public override void RestoreItem(string mailbox, string itemId)
        {
            bool isFinish = false;
            RestoreStart();
            try
            {
                RestoreServiceObj.RestoreItem(mailbox, itemId);
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

            ServiceContext = new ServiceContext(AdminInfo.UserName, AdminInfo.UserPassword, AdminInfo.UserDomain, AdminInfo.OrganizationName, TaskType.Restore);
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
                var result = RestoreFactory.Instance.NewDataAccess();
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
                var itemInformation = GetRestoreItemInformation(mailbox, folderId, item.ItemId);
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

        public virtual void RestoreItem(string mailbox, string itemId)
        {
            IItemData itemDetails = DataAccess.GetItemContent(itemId);
            IItemData item = DataAccess.GetItem(itemId);
            var itemInformation = GetRestoreItemInformation(mailbox, item.ParentFolderId, item.ItemId);
            Destination.WriteItem(itemInformation, itemDetails.Data as byte[]);
        }

        public virtual void RestoreItem(string mailbox, IRestoreItemInformation item)
        {
            IItemData itemDetails = DataAccess.GetItemContent(item.ItemId);
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

        protected virtual void RestoreEnd()
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

        private IRestoreItemInformation GetRestoreItemInformation(string mailAddress, string folderId, string itemId)
        {
            InitMailBoxFolderPathes(mailAddress);
            List<string> result = null;
            if (!_FolderCache.TryGetFolderPath(mailAddress, folderId, out result))
            {
                throw new ArgumentException(string.Format("can find [{0}]'s folder [{1}] pathes", mailAddress, folderId));
            }
            return new RestoreItemInformationImpl()
            {
                FolderPathes = result,
                ItemId = itemId,
                MailAddress = mailAddress
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

        public class TreeNode
        {
            public List<TreeNode> Childrens;
            public IFolderData Folder;
            public string FolderId;

            public TreeNode()
            {
                Childrens = new List<TreeNode>();
            }

            public static Dictionary<string, List<string>> GetEachFolderPath(TreeNode root)
            {
                Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
                List<string> paths = new List<string>();

                var children = root.Childrens;
                foreach (var node in children)
                {
                    paths = new List<string>();
                    paths.Add(node.Folder.DisplayName);
                    TraverseTree(node, result, paths);
                }
                return result;
            }

            private static void TraverseTree(TreeNode child, Dictionary<string, List<string>> result, List<string> paths)
            {
                result.Add(child.Folder.FolderId, paths);
                var children = child.Childrens;
                if (children.Count > 0)
                {
                    foreach (var node in children)
                    {
                        var childpaths = new List<string>();
                        childpaths.AddRange(paths);
                        childpaths.Add(node.Folder.DisplayName);
                        TraverseTree(node, result, childpaths);
                    }
                }
            }

            public static List<string> GetAllFoldersAndChildFolders(TreeNode root, List<string> folderIds)
            {
                HashSet<string> result = new HashSet<string>(folderIds);
                foreach(var id in folderIds)
                {
                    var eachResult = GetAllFoldersAndChildFolders(root, id);
                    foreach(var eachid in eachResult)
                    {
                        if(!result.Contains(eachid))
                        {
                            result.Add(eachid);
                        }
                    }
                }
                return result.ToList();
            }

            public static List<string> GetAllFoldersAndChildFolders(TreeNode root, string folderId)
            {
                var findNode = FindFolderId(root, folderId);
                if (findNode == null)
                    throw new NullReferenceException(string.Format("can't find folder [{0}]", folderId));
                var result = new List<string>();
                GetAllChildFolders(findNode, result);
                return result;
            }

            public static TreeNode FindFolderId(TreeNode node, string folderId)
            {
                if (node.FolderId == folderId)
                    return node;
                foreach(var child in node.Childrens)
                {
                    var result = FindFolderId(child, folderId);
                    if (result != null)
                        return result;
                }
                return null;
            }

            public static void GetAllChildFolders(TreeNode node, List<string> result)
            {
                if (node.Folder != null)
                {
                    result.Add(node.Folder.FolderId);
                    foreach (var child in node.Childrens)
                    {
                        GetAllChildFolders(child, result);
                    }
                }
            }

            public static TreeNode CreateTree(List<IFolderData> folders)
            {
                Dictionary<string, TreeNode> folderId2Node = new Dictionary<string, TreeNode>(folders.Count);
                TreeNode root = null;
                foreach (var folder in folders)
                {
                    var parentId = folder.ParentFolderId;
                    TreeNode node = CreateOrGetParentNode(parentId, folderId2Node);
                    TreeNode childNode = CreateNode(folder, folderId2Node);
                    node.Childrens.Add(childNode);
                }

                foreach (var keyvalue in folderId2Node)
                {
                    if (keyvalue.Value.Folder == null)
                    {
                        root = keyvalue.Value;
                        break;
                    }
                }
                return root;
            }


            private static TreeNode CreateNode(IFolderData folder, Dictionary<string, TreeNode> folderId2Node)
            {
                TreeNode result = null;
                if (!folderId2Node.TryGetValue(folder.FolderId, out result))
                {
                    result = new TreeNode();
                    folderId2Node.Add(folder.FolderId, result);
                }
                result.Folder = folder;
                result.FolderId = folder.FolderId;
                return result;
            }

            private static TreeNode CreateOrGetParentNode(string parentId, Dictionary<string, TreeNode> folderId2Node)
            {
                TreeNode result = null;
                if (!folderId2Node.TryGetValue(parentId, out result))
                {
                    result = new TreeNode();
                    folderId2Node.Add(parentId, result);
                }
                result.FolderId = parentId;
                return result;
            }
        }

        class MailboxFolderCache
        {
            private Dictionary<string, MailboxFolderTreePath> Mailbox2FolderPath = new Dictionary<string, MailboxFolderTreePath>();

            public bool IsMailboxInit(string mailbox)
            {
                return Mailbox2FolderPath.ContainsKey(mailbox);
            }

            public bool TryGetFolderPath(string mailbox, string folderId, out List<string> paths)
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
                    Dictionary<string, List<string>> eachFolderPath = TreeNode.GetEachFolderPath(treeroot);
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
                internal readonly Dictionary<string, List<string>> FolderPaths;
                internal readonly TreeNode Root;
                internal MailboxFolderTreePath(string mailboxAddress, Dictionary<string, List<string>> folderPaths, TreeNode root)
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

            public string MailAddress
            {
                get; set;
            }

            public List<string> FolderPathes { get; set; }
        }
    }
}
