using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using Microsoft.Exchange.WebServices.Data;

namespace EwsService.Impl
{
    public class RestoreDestinationExImpl : IRestoreDestinationEx
    {
        //private ExchangeService CurrentExService;

        public string DestinationMailbox { get; set; }
        public string DestinationFolder { get; set; }

        public ExportType ExportType
        {
            get
            {
                return ExportType.TransferBin;
            }
        }

        private RestoreDestinationImpl _restoreHelper;

        public void DealFolder(string displayName, Stack<IItemBase> dealItemStack)
        {
            
        }

        public void DealItem(string id, string displayName, byte[] itemData, Stack<IItemBase> dealItemStack)
        {
            if(_restoreHelper == null)
            {
                _restoreHelper = new RestoreDestinationImpl();
                _restoreHelper.DesMailboxAddress = DestinationMailbox;
                _restoreHelper.DesFolderDisplayNamePath = DestinationFolder;
            }

            var restoreItemInfo = new RestoreItemInformationImpl() { ItemId = id , DisplayName = displayName };
            var paths = new List<string>(dealItemStack.Count);

            foreach(var itemBase in dealItemStack)
            {
                paths.Insert(0, itemBase.DisplayName);
            }

            if (dealItemStack.Count > 0)
                paths.RemoveAt(dealItemStack.Count - 1);
            
            restoreItemInfo.FolderPathes = paths;
            _restoreHelper.WriteItem(restoreItemInfo, itemData);
        }

        public void DealMailbox(string displayName, Stack<IItemBase> dealItemStack)
        {
            
        }

        public void DealOrganization(string organization, Stack<IItemBase> dealItemStack)
        {
            
        }

        public void SetOtherInformation(params object[] args)
        {
            DestinationMailbox = args[0].ToString();
            DestinationFolder = args[1].ToString();
        }

        public void RestoreComplete(bool success, Exception ex)
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


            public List<string> FolderPathes { get; set; }

            public string DisplayName
            {
                get; set;
            }
        }
    }
}
