using Arcserve.Office365.Exchange.EwsApi.Impl.Increment;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace Arcserve.Office365.Exchange.EwsApi.Extension
{
    internal class EwsServiceAdapterExtension : EwsServiceAdapter, IEwsServiceAdapterExtension<IJobProgress>
    {
        public FindItemsResults<Item> FindItems(FolderId parentFolderId, ViewBase view)
        {
            return _ewsOperator.FindItems(parentFolderId, view);
        }

        public FindItemsResults<Item> FindItems(WellKnownFolderName parentFolderName, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public FindItemsResults<Item> FindItems(WellKnownFolderName parentFolderName, string queryString, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public FindItemsResults<Item> FindItems(FolderId parentFolderId, string queryString, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public FindItemsResults<Item> FindItems(WellKnownFolderName parentFolderName, SearchFilter searchFilter, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public FindItemsResults<Item> FindItems(FolderId parentFolderId, SearchFilter searchFilter, ViewBase view)
        {
            return _ewsOperator.FindItems(parentFolderId, searchFilter, view);
        }

        public Task<FindItemsResults<Item>> FindItemsAsync(FolderId parentFolderId, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public Task<FindItemsResults<Item>> FindItemsAsync(WellKnownFolderName parentFolderName, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public Task<FindItemsResults<Item>> FindItemsAsync(WellKnownFolderName parentFolderName, string queryString, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public Task<FindItemsResults<Item>> FindItemsAsync(FolderId parentFolderId, string queryString, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public Task<FindItemsResults<Item>> FindItemsAsync(WellKnownFolderName parentFolderName, SearchFilter searchFilter, ViewBase view)
        {
            throw new NotImplementedException();
        }

        public Task<FindItemsResults<Item>> FindItemsAsync(FolderId parentFolderId, SearchFilter searchFilter, ViewBase view)
        {
            throw new NotImplementedException();
        }

        

        public void FolderDelete(Folder folder, DeleteMode deleteMode)
        {
            _ewsOperator.FolderDelete(folder, deleteMode);
        }

        public System.Threading.Tasks.Task FolderDeleteAsync(Folder folder, DeleteMode deleteMode)
        {
            throw new NotImplementedException();
        }

        public void FolderEmpty(Folder folder, DeleteMode deleteMode, bool deleteSubFolders)
        {
            _ewsOperator.FolderEmpty(folder, deleteMode, deleteSubFolders);
        }

        public System.Threading.Tasks.Task FolderEmptyAsync(Folder folder, DeleteMode deleteMode, bool deleteSubFolders)
        {
            throw new NotImplementedException();
        }
    }
}
