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

namespace EwsService.Impl
{

    public abstract class EwsBaseOperator
    {
        private ExchangeService service;
        private string Mailbox;
        private EwsServiceArgument EwsArgument;

        public virtual void NewExchangeService(string mailbox, EwsServiceArgument arg, bool isDoAutodiscovery = false)
        {
            DoNewExchangeService(mailbox, arg, isDoAutodiscovery);
        }

        private void DoNewExchangeService(string mailbox, EwsServiceArgument arg, bool isDoAutodiscovery = false)
        {
            service = EwsProxyFactory.CreateExchangeService(arg, mailbox, isDoAutodiscovery);
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

        protected void TryAction(Action operation)
        {
            DoAction(() =>
            {
                operation.Invoke();
            });
        }

        protected T TryFunc<T>(Func<T> operation)
        {
            T result = default(T);
            DoAction(() =>
            {
                result = operation.Invoke();
            });
            return result;
        }


        private static ConcurrentQueue<Exception> ex = new ConcurrentQueue<Exception>();

        private static object _lockAllAction = new object();
        private void DoAction(Action operation)
        {
            _operatorCtrl.DoAction(operation);
        }
        OperatorCtrlBase _operatorCtrl;

        protected EwsBaseOperator()
        {
            var b = new OperatorCtrlBaseImpl();
            var timeOut = new TimeOutOperatorCtrl(b);
            var retry = new RetryOperator(timeOut,
                () =>
                {
                    EwsRequestGate.Instance.Enter();
                },
                (e) =>
            {
                var type = e.GetType();
                if (e is ServiceRequestException)
                {
                    EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(60,
                        ()=> {
                            DoNewExchangeService(Mailbox, EwsArgument, true);
                        }, type)));
                }
                else if(e is OutOfMemoryException)
                {
                    EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(10, null, type)));
                }
                else if(e is TimeoutException)
                {
                    EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(10, null, type)));
                }
                else
                {
                    EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(10, null, type)));
                }
            });
            _operatorCtrl = retry;
        }
    }

    public class EwsLimitOperator : EwsBaseOperator
    {
        public override Folder FolderBind(WellKnownFolderName name, PropertySet propertySet)
        {
            return TryFunc(() =>
            {
                return base.FolderBind(name, propertySet);
            });
        }

        public override Folder FolderBind(WellKnownFolderName name)
        {
            return TryFunc(() =>
            {
                return base.FolderBind(name);
            });
        }

        public override Folder FolderBind(FolderId id, PropertySet propertySet)
        {
            return TryFunc(() =>
            {
                return base.FolderBind(id, propertySet);
            });
        }

        public override Folder FolderBind(FolderId id)
        {
            return TryFunc(() =>
            {
                return base.FolderBind(id);
            });
        }

        public override void FolderSave(Folder folder, FolderId parentFolderId)
        {
            TryAction(() =>
            {
                base.FolderSave(folder, parentFolderId);
            });
        }

        public override void FolderDelete(Folder folder, DeleteMode deleteMode)
        {
            TryAction(() =>
            {
                base.FolderDelete(folder, deleteMode);
            });
        }

        public override FindFoldersResults FindFolders(FolderId parentFolderId, FolderView view)
        {
            return TryFunc(() =>
            {
                return base.FindFolders(parentFolderId, view);
            });
        }

        public override FindFoldersResults FindFolders(FolderId parentFolderId, SearchFilter searchFilter, FolderView view)
        {
            return TryFunc(() =>
            {
                return base.FindFolders(parentFolderId, searchFilter, view);
            });
        }

        public override FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, FolderView view)
        {
            return TryFunc(() =>
            {
                return base.FindFolders(parentFolderName, view);
            });
        }

        public override FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, SearchFilter searchFilter, FolderView view)
        {
            return TryFunc(() =>
            {
                return base.FindFolders(parentFolderName, searchFilter, view);
            });
        }

        public override void Load(Item item, PropertySet propertySet)
        {
            TryAction(() =>
            {
                base.Load(item, propertySet);
            });
        }

        public override FindItemsResults<Item> FindItems(FolderId parentFolderId, SearchFilter searchFilter, ViewBase view)
        {
            return TryFunc(() =>
            {
                return base.FindItems(parentFolderId, searchFilter, view);
            });
        }

        public override FindItemsResults<Item> FindItems(FolderId parentFolderId, ViewBase view)
        {
            return TryFunc(() =>
            {
                return base.FindItems(parentFolderId, view);
            });
        }

        public override void NewExchangeService(string mailbox, EwsServiceArgument arg, bool isDoAutodiscovery = false)
        {
            TryAction(() =>
            {
                base.NewExchangeService(mailbox, arg, isDoAutodiscovery);
            });
        }
    }

}
