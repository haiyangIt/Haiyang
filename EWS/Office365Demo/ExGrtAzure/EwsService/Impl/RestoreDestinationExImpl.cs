using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using Microsoft.Exchange.WebServices.Data;
using DataProtectInterface.Util;
using EwsServiceInterface;

namespace EwsService.Impl
{
    public class RestoreDestinationExImpl : IRestoreDestinationEx
    {
        public RestoreDestinationExImpl(EwsServiceArgument argument, IDataAccess dataAccess)
        {
            _argument = argument;
            _dataAccess = dataAccess;
        }

        protected readonly EwsServiceArgument _argument;
        protected readonly IDataAccess _dataAccess;

        public string DestinationMailbox { get; set; }
        public string DestinationFolder {
            get; set;
        }
        private List<IFolderDataBase> DestinationFolderDataBase { get; set; }

        public ExportType ExportType
        {
            get
            {
                return ExportType.TransferBin;
            }
            set
            {

            }
        }

        protected RestoreDestinationImpl _restoreHelperCache;

        public void DealFolder(string displayName, Stack<IItemBase> dealItemStack)
        {
            
        }

        protected virtual void CreateHelperCache(Stack<IItemBase> dealItemStack)
        {
            if (_restoreHelperCache == null)
            {
                _restoreHelperCache = new RestoreDestinationImpl(_argument, _dataAccess);
                _restoreHelperCache.DesMailboxAddress = DestinationMailbox;
                _restoreHelperCache.DesFolderDisplayNamePath = DestinationFolder;
            }
        }

        protected virtual List<IFolderDataBase> GetPaths(Stack<IItemBase> dealItemStack)
        {
            var paths = new List<IFolderDataBase>(dealItemStack.Count);

            foreach (var itemBase in dealItemStack)
            {
                IFolderDataBase folderDataBase = FolderClassUtil.NewFolderDataBase(itemBase);
                paths.Insert(0, folderDataBase);
            }

            if (dealItemStack.Count > 0)
                paths.RemoveAt(dealItemStack.Count - 1);
            return paths;
        }

        public virtual void DealItem(string id, string displayName, byte[] itemData, Stack<IItemBase> dealItemStack)
        {
            CreateHelperCache(dealItemStack);

            var item = dealItemStack.Pop();
            dealItemStack.Push(item);
            var itemClass = ItemClassUtil.GetItemClass(item);

            var restoreItemInfo = new RestoreItemInformationImpl() { ItemId = id , DisplayName = displayName, ItemClass = itemClass };
            
            restoreItemInfo.FolderPathes = GetPaths(dealItemStack);
            _restoreHelperCache.WriteItem(restoreItemInfo, itemData);
        }

        public virtual void DealMailbox(string displayName, Stack<IItemBase> dealItemStack)
        {
            
        }

        public void DealOrganization(string organization, Stack<IItemBase> dealItemStack)
        {
            
        }

        public virtual void SetOtherInformation(params object[] args)
        {
            DestinationMailbox = args[0].ToString();
            DestinationFolder = args[1].ToString();
        }

        public void RestoreComplete(bool success, IRestoreServiceEx restoreService, Exception ex)
        {
        }

        public void Dispose()
        {
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

    public class RestoreDestinationOrgExImpl : RestoreDestinationExImpl
    {
        public RestoreDestinationOrgExImpl(EwsServiceArgument argument, IDataAccess dataAccess) : base(argument, dataAccess)
        {
        }

        public override void SetOtherInformation(params object[] args)
        {
            DestinationFolder = args[0].ToString();
        }


        public override void DealMailbox(string displayName, Stack<IItemBase> dealItemStack)
        {

        }

        Dictionary<string, RestoreDestinationImpl> helperDic = new Dictionary<string, RestoreDestinationImpl>();
        protected override void CreateHelperCache(Stack<IItemBase> dealItemStack)
        {
            var mailAddress = "";
            foreach (var itemBase in dealItemStack)
            {
                if (itemBase.ItemKind == ItemKind.Mailbox)
                {
                    var des = MailClassUtil.GetMailboxData(itemBase);
                    mailAddress = des.MailAddress;
                    break;
                }
            }
            if (string.IsNullOrEmpty(mailAddress))
                throw new ArgumentException();

            RestoreDestinationImpl instance = null;
            if(!helperDic.TryGetValue(mailAddress, out instance))
            {
                instance = new RestoreDestinationImpl(_argument, _dataAccess);
                instance.DesMailboxAddress = mailAddress;
                instance.DesFolderDisplayNamePath = DestinationFolder;
                helperDic.Add(mailAddress, instance);
            }
            _restoreHelperCache = instance;
        }

        protected override List<IFolderDataBase> GetPaths(Stack<IItemBase> dealItemStack)
        {
            var paths = new List<IFolderDataBase>(dealItemStack.Count);
            foreach (var itemBase in dealItemStack)
            {
                if (itemBase.ItemKind == ItemKind.Folder)
                {
                    IFolderDataBase folderDataBase = FolderClassUtil.NewFolderDataBase(itemBase);
                    paths.Insert(0, folderDataBase);
                }
            }

            return paths;
        }
    }
}
