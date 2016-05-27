using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Arcserve.Office365.Exchange.EwsApi.Impl.Common;
using Arcserve.Office365.Exchange.Topaz;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using Arcserve.Office365.Exchange.EwsApi.Increment;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Log;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Increment
{
    public abstract class EwsBaseOperator
    {
        private ExchangeService service;
        private string Mailbox;
        private EwsServiceArgument EwsArgument;

        public virtual string NewExchangeService(string mailbox, EwsServiceArgument arg, bool isDoAutodiscovery = false)
        {
            return DoNewExchangeService(mailbox, arg, isDoAutodiscovery);

        }

        private string DoNewExchangeService(string mailbox, EwsServiceArgument arg, bool isDoAutodiscovery = false)
        {
            Mailbox = mailbox;
            EwsArgument = arg;
            service = EwsProxyFactory.CreateExchangeService(arg, mailbox, isDoAutodiscovery);
            return service.Url.AbsoluteUri;
        }

        private static SecureString StringToSecureString(string str)
        {
            SecureString ss = new SecureString();
            char[] passwordChars = str.ToCharArray();

            foreach (char c in passwordChars)
            {
                ss.AppendChar(c);
            }
            return ss;
        }

        public virtual ICollection<IMailboxDataSync> GetAllMailbox(string adminName, string adminPassword)
        {
            const string liveIDConnectionUri = "https://outlook.office365.com/PowerShell-LiveID";
            const string schemaUri = "http://schemas.microsoft.com/powershell/Microsoft.Exchange";
            PSCredential credentials = new PSCredential(adminName, StringToSecureString(adminPassword));

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(
        new Uri(liveIDConnectionUri),
        schemaUri, credentials);
            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Basic;

            using (Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo))
            {
                using (Pipeline pipe = runspace.CreatePipeline())
                {

                    Command CommandGetMailbox = new Command("Get-Mailbox");
                    CommandGetMailbox.Parameters.Add("RecipientTypeDetails", "UserMailbox");
                    pipe.Commands.Add(CommandGetMailbox);

                    var props = new string[] { "Name", "DisplayName", "UserPrincipalName", "Guid" };
                    Command CommandSelect = new Command("Select-Object");
                    CommandSelect.Parameters.Add("Property", props);
                    pipe.Commands.Add(CommandSelect);


                    runspace.Open();

                    var information = pipe.Invoke();
                    List<IMailboxDataSync> result = new List<IMailboxDataSync>(information.Count);
                    string displayName = string.Empty;
                    string address = string.Empty;
                    string name = string.Empty;
                    string guid = string.Empty;
                    foreach (PSObject eachUserMailBox in information)
                    {
                        displayName = string.Empty;
                        address = string.Empty;
                        name = string.Empty;
                        guid = string.Empty;

                        foreach (PSPropertyInfo propertyInfo in eachUserMailBox.Properties)
                        {
                            if (propertyInfo.Name == "DisplayName")
                                displayName = propertyInfo.Value.ToString();
                            if (propertyInfo.Name == "UserPrincipalName")
                                address = propertyInfo.Value.ToString().ToLower();
                            if (propertyInfo.Name == "Guid")
                                guid = propertyInfo.Value.ToString();
                            if (propertyInfo.Name == "Name")
                                name = propertyInfo.Value.ToString();

                        }

                        //if (IsNeedGenerateMailbox(address) && address.ToLower() == "haiyang.ling@arcserve.com") // todo remove the specific mail address.
                        result.Add(new MailboxDataSyncBase(displayName, address) { Name = name, Id = guid });
                    }
                    return result;
                }
            }
        }



        public virtual void LoadFolderProperties(Folder folder, PropertySet folderPropertySet)
        {
            folder.Load(folderPropertySet);
        }

        public virtual ChangeCollection<FolderChange> SyncFolderHierarchy(string lastSyncStatus)
        {
            var rootFolder = FolderBind(WellKnownFolderName.MsgFolderRoot, BasePropertySet.IdOnly);
            return service.SyncFolderHierarchy(rootFolder.Id, BasePropertySet.IdOnly, lastSyncStatus);
        }

        public virtual Folder FolderBind(WellKnownFolderName name, PropertySet propertySet)
        {
            return Folder.Bind(service, name, propertySet);
        }

        public virtual Folder FolderBind(WellKnownFolderName name)
        {
            return Folder.Bind(service, name);
        }
        public virtual Folder FolderBind(FolderId id)
        {
            return Folder.Bind(service, id);
        }
        public virtual Folder FolderBind(FolderId id, PropertySet propertySet)
        {
            return Folder.Bind(service, id);
        }

        public virtual void FolderSave(Folder folder, FolderId parentFolderId)
        {
            folder.Save(parentFolderId);
        }

        public virtual void FolderSave(string folderName, string folderClass, FolderId parentFolderId)
        {
            Folder folder = new Folder(service);
            folder.DisplayName = folderName;
            folder.FolderClass = folderClass;
            folder.Save(parentFolderId);
        }


        //
        // Summary:
        //     Obtains a list of folders by searching the sub-folders of the specified folder.
        //
        // Parameters:
        //   parentFolderName:
        //     The name of the folder in which to search for folders.
        //
        //   view:
        //     The view controlling the number of folders returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        public virtual FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, FolderView view)
        {
            return service.FindFolders(parentFolderName, view);
        }
        //
        // Summary:
        //     Obtains a list of folders by searching the sub-folders of the specified folder.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for folders.
        //
        //   view:
        //     The view controlling the number of folders returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        public virtual FindFoldersResults FindFolders(FolderId parentFolderId, FolderView view)
        {
            return service.FindFolders(parentFolderId, view);
        }
        //
        // Summary:
        //     Obtains a list of folders by searching the sub-folders of the specified folder.
        //
        // Parameters:
        //   parentFolderName:
        //     The name of the folder in which to search for folders.
        //
        //   searchFilter:
        //     The search filter. Available search filter classes include SearchFilter.IsEqualTo,
        //     SearchFilter.ContainsSubstring and SearchFilter.SearchFilterCollection
        //
        //   view:
        //     The view controlling the number of folders returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        public virtual FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, SearchFilter searchFilter, FolderView view)
        {
            return service.FindFolders(parentFolderName, searchFilter, view);
        }
        //
        // Summary:
        //     Obtains a list of folders by searching the sub-folders of the specified folder.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for folders.
        //
        //   searchFilter:
        //     The search filter. Available search filter classes include SearchFilter.IsEqualTo,
        //     SearchFilter.ContainsSubstring and SearchFilter.SearchFilterCollection
        //
        //   view:
        //     The view controlling the number of folders returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        public virtual FindFoldersResults FindFolders(FolderId parentFolderId, SearchFilter searchFilter, FolderView view)
        {
            return service.FindFolders(parentFolderId, searchFilter, view);
        }

        public virtual void FolderDelete(Folder folder, DeleteMode deleteMode)
        {
            folder.Delete(deleteMode);
        }

        public virtual ChangeCollection<ItemChange> SyncFolderItems(FolderId folderId, string lastSyncStatus)
        {
            return service.SyncFolderItems(folderId, BasePropertySet.IdOnly, null, CloudConfig.Instance.MaxItemChangesReturn, SyncFolderItemsScope.NormalItems, lastSyncStatus);
        }

        //
        // Summary:
        //     Loads the specified set of properties. Calling this method results in a call
        //     to EWS.
        //
        // Parameters:
        //   propertySet:
        //     The properties to load.
        public virtual void Load(Item item, PropertySet propertySet)
        {
            item.Load(propertySet);
        }
        //
        // Summary:
        //     Obtains a list of items by searching the contents of a specific folder. Calling
        //     this method results in a call to EWS.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for items.
        //
        //   view:
        //     The view controlling the number of items returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        public virtual FindItemsResults<Item> FindItems(FolderId parentFolderId, ViewBase view)
        {
            return service.FindItems(parentFolderId, view);
        }

        //
        // Summary:
        //     Obtains a list of items by searching the contents of a specific folder. Calling
        //     this method results in a call to EWS.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for items.
        //
        //   searchFilter:
        //     The search filter. Available search filter classes include SearchFilter.IsEqualTo,
        //     SearchFilter.ContainsSubstring and SearchFilter.SearchFilterCollection
        //
        //   view:
        //     The view controlling the number of items returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        public virtual FindItemsResults<Item> FindItems(FolderId parentFolderId, SearchFilter searchFilter, ViewBase view)
        {
            return service.FindItems(parentFolderId, searchFilter, view);
        }

        public virtual void LoadPropertiesForItems(IEnumerable<Item> items, PropertySet itemPropertySet)
        {
            var response = service.LoadPropertiesForItems(items, itemPropertySet);

            foreach (var item in response)
            {
                if (item.Result == ServiceResult.Error)
                {
                    LogFactory.LogInstance.WriteLog("", LogLevel.ERR, item.GetDetailInformation());
                }
                else if (item.Result == ServiceResult.Warning)
                {
                    LogFactory.LogInstance.WriteLog("", LogLevel.WARN, item.GetDetailInformation());
                }
            }
        }

        public virtual byte[] ExportItem(string sItemId)
        {
            return ExportUploadHelper.ExportItemPost(Enum.GetName(typeof(ExchangeVersion), service.RequestedServerVersion), sItemId, EwsArgument);
        }

        public virtual int ExportItems(IEnumerable<IItemDataSync> items, IExportItemsOper exportItemOper)
        {
            return ExportUploadHelper.ExportItemsPost(Enum.GetName(typeof(ExchangeVersion), service.RequestedServerVersion), items, EwsArgument, exportItemOper);
        }

        public virtual void ImportItem(string parentFolderId, Stream stream, EwsServiceArgument argument)
        {
            ExportUploadHelper.UploadItemPost(Enum.GetName(typeof(ExchangeVersion),
                service.RequestedServerVersion), parentFolderId, CreateActionType.CreateNew, string.Empty, stream, argument);
        }

        public virtual void ImportItem(string parentFolderId, byte[] itemData, EwsServiceArgument argument)
        {
            ExportUploadHelper.UploadItemPost(Enum.GetName(typeof(ExchangeVersion),
                service.RequestedServerVersion), parentFolderId, CreateActionType.CreateNew, string.Empty, itemData, argument);
        }

        protected void TryAction(Action operation, string operationName)
        {
            OperatorCtrlBase.DoActionWithRetryTimeOut(() =>
            {
                operation.Invoke();
            }, operationName);
        }

        protected T TryFunc<T>(Func<T> operation, string operationName)
        {
            T result = default(T);
            OperatorCtrlBase.DoActionWithRetryTimeOut(() =>
            {
                result = operation.Invoke();
            }, operationName);
            return result;
        }

        protected EwsBaseOperator()
        {

        }
    }

    public class EwsLimitOperator : EwsBaseOperator
    {
        public EwsLimitOperator() : base() { }

        public override ICollection<IMailboxDataSync> GetAllMailbox(string adminName, string adminPassword)
        {
            return TryFunc(() =>
            {
                return base.GetAllMailbox(adminName, adminPassword);
            }, "GetAllMailbox");
        }
        public override Folder FolderBind(WellKnownFolderName name, PropertySet propertySet)
        {
            return TryFunc(() =>
            {
                return base.FolderBind(name, propertySet);
            }, "FolderBind");
        }

        public override Folder FolderBind(WellKnownFolderName name)
        {
            return TryFunc(() =>
            {
                return base.FolderBind(name);
            }, "FolderBind");
        }

        public override Folder FolderBind(FolderId id, PropertySet propertySet)
        {
            return TryFunc(() =>
            {
                return base.FolderBind(id, propertySet);
            }, "FolderBind");
        }

        public override Folder FolderBind(FolderId id)
        {
            return TryFunc(() =>
            {
                return base.FolderBind(id);
            }, "FolderBind");
        }

        public override void FolderSave(Folder folder, FolderId parentFolderId)
        {
            TryAction(() =>
            {
                base.FolderSave(folder, parentFolderId);
            }, "FolderSave");
        }

        public override void FolderDelete(Folder folder, DeleteMode deleteMode)
        {
            TryAction(() =>
            {
                base.FolderDelete(folder, deleteMode);
            }, "FolderDelete");
        }

        public override FindFoldersResults FindFolders(FolderId parentFolderId, FolderView view)
        {
            return TryFunc(() =>
            {
                return base.FindFolders(parentFolderId, view);
            }, "FindFolders");
        }

        public override FindFoldersResults FindFolders(FolderId parentFolderId, SearchFilter searchFilter, FolderView view)
        {
            return TryFunc(() =>
            {
                return base.FindFolders(parentFolderId, searchFilter, view);
            }, "FindFolders");
        }

        public override FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, FolderView view)
        {
            return TryFunc(() =>
            {
                return base.FindFolders(parentFolderName, view);
            }, "FindFolders");
        }

        public override FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, SearchFilter searchFilter, FolderView view)
        {
            return TryFunc(() =>
            {
                return base.FindFolders(parentFolderName, searchFilter, view);
            }, "FindFolders");
        }

        public override void Load(Item item, PropertySet propertySet)
        {
            TryAction(() =>
            {
                base.Load(item, propertySet);
            }, "Load");
        }

        public override FindItemsResults<Item> FindItems(FolderId parentFolderId, SearchFilter searchFilter, ViewBase view)
        {
            return TryFunc(() =>
            {
                return base.FindItems(parentFolderId, searchFilter, view);
            }, "FindItems");
        }

        public override FindItemsResults<Item> FindItems(FolderId parentFolderId, ViewBase view)
        {
            return TryFunc(() =>
            {
                return base.FindItems(parentFolderId, view);
            }, "FindItems");
        }

        public override string NewExchangeService(string mailbox, EwsServiceArgument arg, bool isDoAutodiscovery = false)
        {
            return TryFunc(() =>
            {
                return base.NewExchangeService(mailbox, arg, isDoAutodiscovery);
            }, "NewExchangeService");
        }

        public override void FolderSave(string folderName, string folderClass, FolderId parentFolderId)
        {
            TryAction(() =>
            {
                base.FolderSave(folderName, folderClass, parentFolderId);
            }, "FolderSave");
        }


        //public override void (string sItemId, EwsServiceArgument argument)
        //{
        //    return TryFunc(() =>
        //    {
        //        return base.ExportItem(sItemId, argument);
        //    }, "ExportItem");
        //}

        public override void ImportItem(string parentFolderId, byte[] itemData, EwsServiceArgument argument)
        {
            TryAction(() =>
            {
                base.ImportItem(parentFolderId, itemData, argument);
            }, "ImportItem");
        }

        public override void ImportItem(string parentFolderId, Stream stream, EwsServiceArgument argument)
        {
            TryAction(() =>
            {
                base.ImportItem(parentFolderId, stream, argument);
            }, "ImportItem");
        }

        //public override IEnumerable<ItemDatas> ExportItems(IEnumerable<Item> items)
        //{
        //    return TryFunc(() =>
        //    {
        //        return base.ExportItems(items);
        //    }, "ExportItems");
        //}

        public override void LoadFolderProperties(Folder folder, PropertySet folderPropertySet)
        {
            TryAction(() =>
            {
                base.LoadFolderProperties(folder, folderPropertySet);
            }, "LoadFolderProperties");
        }

        public override ChangeCollection<FolderChange> SyncFolderHierarchy(string lastSyncStatus)
        {
            return TryFunc(() =>
            {
                return base.SyncFolderHierarchy(lastSyncStatus);
            }, "SyncFolderHierarchy");
        }

        public override ChangeCollection<ItemChange> SyncFolderItems(FolderId folderId, string lastSyncStatus)
        {
            return TryFunc(() =>
            {
                return base.SyncFolderItems(folderId, lastSyncStatus);
            }, "SyncFolderItems");
        }

        public override byte[] ExportItem(string sItemId)
        {
            return TryFunc(() =>
            {
                return base.ExportItem(sItemId);
            }, "ExportItem");
        }

        public override int ExportItems(IEnumerable<IItemDataSync> items, IExportItemsOper exportItemOper)
        {
            return TryFunc(() =>
            {
                return base.ExportItems(items, exportItemOper);
            }, "ExportItems");
        }

        public override void LoadPropertiesForItems(IEnumerable<Item> items, PropertySet itemPropertySet)
        {
            TryAction(() =>
            {
                base.LoadPropertiesForItems(items, itemPropertySet);
            }, "LoadPropertiesForItems");
        }
    }

}
