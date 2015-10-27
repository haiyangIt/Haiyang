using ExGrtAzure.EWS.FolderOperator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExGrtAzure.EWS.ItemOperator;
using Microsoft.Exchange.WebServices.Data;

namespace ExGrtAzure.EWS.Catalog.Operator
{
    public class FolderOperatorImpl : IFolder
    {
        private static HashSet<string> _validFolderType;
        private static HashSet<string> ValidFolderType
        {
            get
            {
                if(_validFolderType == null)
                {
                    _validFolderType = new HashSet<string>();
                    _validFolderType.Add("IPF.Note");
                    _validFolderType.Add("IPF.Appointment");
                    _validFolderType.Add("IPF.Contact");
                }
                return _validFolderType;
            }
        }

        private readonly string _organization;
        public FolderOperatorImpl(ExchangeService service, string organization)
        {
            CurrentExchangeService = service;
            _organization = organization;
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
            return Folder.Bind(CurrentExchangeService, WellKnownFolderName.MsgFolderRoot);
        }

        
        public bool IsFolderNeedGenerateCatalog(Folder folder)
        {
            return ValidFolderType.Contains(folder.FolderClass);
        }

        public IItem NewItemOperatorInstance()
        {
            return new ItemOperatorImpl(CurrentExchangeService, _organization);
        }
    }
}