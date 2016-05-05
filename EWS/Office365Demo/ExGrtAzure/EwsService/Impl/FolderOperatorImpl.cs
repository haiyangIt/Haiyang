using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using EwsServiceInterface;
using System.Threading;
using DataProtectInterface.Util;
using EwsDataInterface;
using DataProtectInterface;

namespace EwsService.Impl
{
    public class FolderOperatorImpl : IFolder
    {
        private PropertyDefinition[] _folderProperties;
        private PropertyDefinition[] FolderProperties
        {
            get
            {

                if (_folderProperties == null)
                {
                    _folderProperties = new PropertyDefinition[]
                    {
                        FolderSchema.DisplayName,
                        FolderSchema.ParentFolderId,
                        FolderSchema.ChildFolderCount,
                        FolderSchema.FolderClass,
                        FolderSchema.TotalCount
                    };
                }
                return _folderProperties;

            }
        }

        private PropertySet _folderPropertySet;
        private PropertySet FolderPropertySet
        {
            get
            {
                if(_folderPropertySet == null)
                {
                    _folderPropertySet = new PropertySet(FolderProperties);
                }
                return _folderPropertySet;
            }
        }

        public FolderOperatorImpl(ExchangeService service)
        {
            CurrentExchangeService = service;
        }


        public ExchangeService CurrentExchangeService
        {
            get; private set;
        }

        public List<Folder> GetChildFolder(Folder parentFolder)
        {
            const int pageSize = 100;
            int offset = 0;
            bool moreItems = true;
            List<Folder> result = new List<Folder>(parentFolder.ChildFolderCount);
            while (moreItems)
            {
                FolderView oView = new FolderView(pageSize, offset, OffsetBasePoint.Beginning);
                oView.PropertySet = FolderPropertySet;
                FindFoldersResults findResult = parentFolder.FindFolders(oView);
                result.AddRange(findResult.Folders);
                if (!findResult.MoreAvailable)
                    moreItems = false;

                if (moreItems)
                    offset += pageSize;
            }
            return result;
        }

        public string GetFolderDisplayName(Folder folder)
        {
            return folder.DisplayName;
        }

        public Folder GetRootFolder()
        {
            return Folder.Bind(CurrentExchangeService, WellKnownFolderName.MsgFolderRoot, FolderPropertySet);
        }


        public bool IsFolderNeedGenerateCatalog(Folder folder)
        {
            return FolderClassUtil.IsFolderValid(folder.FolderClass);
        }

        public FolderId CreateChildFolder(IFolderDataBase folderData, FolderId parentFolderId)
        {
            Folder folder = new Folder(CurrentExchangeService);
            folder.DisplayName = folderData.DisplayName;
            folder.FolderClass = folderData.FolderType;
            folder.Save(parentFolderId);
            return FindFolder(folderData, parentFolderId);
        }

        public FolderId FindFolder(IFolderDataBase folderData, FolderId parentFolderId, int findCount = 0)
        {
            FolderView view = new FolderView(1);
            view.PropertySet = new PropertySet(BasePropertySet.IdOnly);
            view.Traversal = FolderTraversal.Shallow;
            SearchFilter filter = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderData.DisplayName);
            FindFoldersResults results = CurrentExchangeService.FindFolders(parentFolderId, filter, view);

            if (results.TotalCount > 1)
            {
                throw new InvalidOperationException("Find more than 1 folder.");
            }
            else if (results.TotalCount == 0)
            {
                if (findCount > 3)
                {
                    return null;
                }
                Thread.Sleep(500);
                return FindFolder(folderData, parentFolderId, ++findCount);
            }
            else
            {
                foreach (var result in results)
                {
                    return result.Id;
                }
                throw new InvalidOperationException("Thread sleep time is too short.");
            }
        }

        public void DeleteFolder(FolderId findFolderId, DeleteMode deleteMode)
        {
            Folder folder = Folder.Bind(CurrentExchangeService, findFolderId);
            folder.Delete(deleteMode);
        }

        public List<Folder> GetChildFolder(string parentFolderId)
        {
            const int pageSize = 100;
            int offset = 0;
            bool moreItems = true;
            List<Folder> result = new List<Folder>();
            var parentFolder = new FolderId(parentFolderId);
            //var parentFolderObj = Folder.Bind(CurrentExchangeService, parentFolder);
            // return GetChildFolder(parentFolderObj);
            while (moreItems)
            {
                FolderView oView = new FolderView(pageSize, offset, OffsetBasePoint.Beginning);
                oView.PropertySet = FolderPropertySet;
                FindFoldersResults findResult = CurrentExchangeService.FindFolders(parentFolder, oView);
                result.AddRange(findResult.Folders);
                if (!findResult.MoreAvailable)
                    moreItems = false;

                if (moreItems)
                    offset += pageSize;
            }
            return result;
        }
    }
}