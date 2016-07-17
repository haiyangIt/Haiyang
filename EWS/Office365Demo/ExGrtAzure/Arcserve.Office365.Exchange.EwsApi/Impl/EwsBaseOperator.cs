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
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using System.Xml;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Increment
{
    internal abstract class EwsBaseOperator
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

        public virtual ICollection<IMailboxDataSync> GetAllMailbox(string adminName, string adminPassword, IEnumerable<string> mailboxes)
        {
            var mailboxResult = EwsServiceExtension.GetAllMailbox(adminName, adminPassword, mailboxes);
            var result = new List<IMailboxDataSync>(mailboxResult.Count);
            foreach (var item in mailboxResult)
            {
                result.Add(new MailboxDataSyncBase(item.DisplayName, item.MailAddress) { Name = item.Name, Id = item.Id });
            }
            return result;
        }

        public virtual void FolderCreate(string folderName, string folderType, Folder parentFolder)
        {
            Folder folder = new Folder(service);
            folder.DisplayName = folderName;
            folder.FolderClass = folderType;
            folder.Save(parentFolder.Id);
        }

        public virtual void LoadFolderProperties(Folder folder, PropertySet folderPropertySet)
        {
            try
            {
                folder.Load(folderPropertySet);
            }
            catch (ArgumentException e)
            {
                LogFactory.LogInstance.WriteException(LogLevel.DEBUG, "Folder load error.", e, e.Message);
                if (e.TargetSite.DeclaringType.FullName == "System.Enum+EnumResult" && e.TargetSite.MemberType == System.Reflection.MemberTypes.Method && e.TargetSite.Name == "SetFailure" && e.Message.IndexOf("Requested value ") >= 0)
                {
                    PropertySet set = new PropertySet(folderPropertySet);
                    set.Remove(FolderSchema.WellKnownFolderName);
                    folder.Load(set);
                }
                else
                    throw e;
            }
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

        public virtual void FolderEmpty(Folder folder, DeleteMode deleteMode, bool deleteSubFolders)
        {
            folder.Empty(deleteMode, deleteSubFolders);
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

        internal void ImportItems(IEnumerable<ImportItemStatus> partition, Folder folder)
        {
            throw new NotImplementedException();
            //return ExportUploadHelper.UploadItemsPost(Enum.GetName(typeof(ExchangeVersion), service.RequestedServerVersion), items, EwsArgument, exportItemOper);
        }

        public virtual void ImportItem(string parentFolderId, Stream stream)
        {
            ExportUploadHelper.UploadItemPost(Enum.GetName(typeof(ExchangeVersion),
                service.RequestedServerVersion), parentFolderId, CreateActionType.CreateNew, string.Empty, stream, EwsArgument);
        }

        public virtual void ImportItem(string parentFolderId, byte[] itemData)
        {
            ExportUploadHelper.UploadItemPost(Enum.GetName(typeof(ExchangeVersion),
                service.RequestedServerVersion), parentFolderId, CreateActionType.CreateNew, string.Empty, itemData, EwsArgument);
        }

        protected void TryAction(Action operation, string operationName)
        {
            OperatorCtrlBase.DoActionWithRetryTimeOut(() =>
            {
                operation.Invoke();
            }, operationName, IsExceptionNeedSuspendRequest, IsExceptionCanRetry);
        }

        private static string[] SuspendArray = new string[]
        {
            "An existing connection was forcibly closed by the remote host",
            "The underlying connection was closed",
            "The mailbox database is temporarily unavailable",
            "The connection was closed."
        };

        private static string[] RetryArray = new string[]
        {
            "An existing connection was forcibly closed by the remote host",
            "The underlying connection was closed",
            "The mailbox database is temporarily unavailable",
            "The connection was closed.",
            "Unexpected end of file has occurred",
            "Received an unexpected EOF or 0 bytes from the transport stream"
        };

        private static bool IsExceptionCanRetry(Exception e)
        {
            var result1 = ((e is ServiceRequestException) ||
               (e is WebException) ||
               (e is SocketException) ||
               (e is ServiceResponseException) ||
               (e is IOException) ||
               (e is XmlException));
            var result2 = RetryArray.Any(e.Message.Contains);
            var result3 = (e is TimeoutException && e.Message == TimeOutOperatorCtrl.RetryMessage);

            return (result1 && result2) || result3;
        }

        private static bool IsExceptionNeedSuspendRequest(Exception e)
        {
            return ((e is ServiceRequestException) ||
                (e is WebException) ||
                (e is SocketException) ||
                (e is ServiceResponseException) ||
                (e is IOException)) && (
                    SuspendArray.Any(e.Message.Contains)
                );
        }

        protected T TryFunc<T>(Func<T> operation, string operationName)
        {
            T result = default(T);
            OperatorCtrlBase.DoActionWithRetryTimeOut(() =>
            {
                result = operation.Invoke();
            }, operationName, IsExceptionNeedSuspendRequest, IsExceptionCanRetry);
            return result;
        }

        protected EwsBaseOperator()
        {

        }
    }

    internal class EwsLimitOperator : EwsBaseOperator
    {
        public EwsLimitOperator() : base() { }

        public override ICollection<IMailboxDataSync> GetAllMailbox(string adminName, string adminPassword, IEnumerable<string> mailboxes)
        {
            return TryFunc(() =>
            {
                return base.GetAllMailbox(adminName, adminPassword, mailboxes);
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

        public override void FolderCreate(string folderName, string folderType, Folder parentFolder)
        {
            TryAction(() =>
            {
                base.FolderCreate(folderName, folderType, parentFolder);
            }, "FolderCreate");

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


        public override void FolderEmpty(Folder folder, DeleteMode deleteMode, bool deleteSubFolders)
        {
            TryAction(() =>
            {
                base.FolderEmpty(folder, deleteMode, deleteSubFolders);
            }, "FolderEmpty");

        }

        //public override void (string sItemId, EwsServiceArgument argument)
        //{
        //    return TryFunc(() =>
        //    {
        //        return base.ExportItem(sItemId, argument);
        //    }, "ExportItem");
        //}

        public override void ImportItem(string parentFolderId, byte[] itemData)
        {
            TryAction(() =>
            {
                base.ImportItem(parentFolderId, itemData);
            }, "ImportItem");
        }

        public override void ImportItem(string parentFolderId, Stream stream)
        {
            TryAction(() =>
            {
                base.ImportItem(parentFolderId, stream);
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
