using EwsFrame;
using EwsFrame.Util;
using EwsService.Common;
using EwsServiceInterface;
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

namespace EwsService.Impl
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

        public virtual byte[] ExportItem(string sItemId, EwsServiceArgument argument)
        {
            return ExportUploadHelper.ExportItemPost(Enum.GetName(typeof(ExchangeVersion), service.RequestedServerVersion), sItemId, argument);
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


        public override byte[] ExportItem(string sItemId, EwsServiceArgument argument)
        {
            return TryFunc(() =>
            {
                return base.ExportItem(sItemId, argument);
            }, "ExportItem");
        }

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
    }

}
